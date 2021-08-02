using System.Threading;
using System.Threading.Tasks;
using Accord.Bot.Models;
using Accord.Bot.Services;
using Remora.Commands.Results;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Accord.Bot.Infrastructure
{
    public class CommandExecutionEventRespondHandler : IPreExecutionEvent, IPostExecutionEvent
    {
        private readonly BotState _botState;
        private readonly UserFeedbackService _userFeedbackService;

        public CommandExecutionEventRespondHandler(BotState botState, UserFeedbackService userFeedbackService)
        {
            _botState = botState;
            _userFeedbackService = userFeedbackService;
        }
        public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = new CancellationToken())
        {
            if (commandResult.IsSuccess)
            {
                if (commandResult is not Result<IUserMessage> messageResult)
                {
                    return Result.FromSuccess();
                }
                IResult feedbackResult = await _userFeedbackService.RespondAsync(messageResult.Entity!, ct);
                return feedbackResult.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(feedbackResult.Error!);
            }

            while (commandResult.Inner is not null)
            {
                commandResult = commandResult.Inner;
            }

            var error = commandResult.Error!;
            switch (error)
            {
                case InvalidOperationError:
                case ConditionNotSatisfiedError:
                    var sendWarning = await _userFeedbackService.RespondStateAsync(
                        new WarningMessage(error.Message),
                        ct
                    );
                    return sendWarning.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(sendWarning);
                case ParameterParsingError:
                case AmbiguousCommandInvocationError:
                case { } when error.GetType().IsGenericType &&
                              error.GetType().GetGenericTypeDefinition() == typeof(ParsingError<>):
                    // Alert the user, and don't complete the transaction
                    var sendError = await _userFeedbackService.RespondStateAsync(
                        new ErrorMessage(error.Message),
                        ct
                    );

                    return sendError.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(sendError);
                default:
                    return Result.FromError(commandResult.Error!);
            }
        }


        public Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct = new CancellationToken()) =>
            Task.FromResult(!_botState.IsReady
                ? Result.FromError(new InvalidOperationError("Bot is not ready yet"))
                : Result.FromSuccess());
    }
}