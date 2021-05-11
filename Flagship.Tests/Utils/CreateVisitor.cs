using Flagship;
using Flagship.Model;
using Flagship.Services;
using Flagship.Services.Decision;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.HitSender;
using Flagship.Services.Logger;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Tests.Utils
{
    class CreateVisitor
    {
        public static IFlagshipVisitor Create(string environmentId, string apiKey, string visitorId, IDictionary<string,object> context, DecisionResponse mockResponse, HttpClient customClient = null)
        {
            var mockClient = new Mock<IDecisionManager>();
            mockClient.Setup(foo => foo.GetResponse(It.IsAny<DecisionRequest>())).Returns(Task.FromResult(mockResponse));

            var logger = new DefaultLogger();
            var errorHandler = new DefaultExceptionHandler();

            var flagshipContext = new FlagshipContext(environmentId, apiKey);
            var sender = new Sender(flagshipContext);
            if (customClient != null)
            {
                var senderClient = sender.GetType().GetField("httpClient", System.Reflection.BindingFlags.NonPublic
        | System.Reflection.BindingFlags.Instance);
                senderClient.SetValue(sender, customClient);
            }

            var contextSender = flagshipContext.GetType().GetField("Sender", System.Reflection.BindingFlags.Public
        | System.Reflection.BindingFlags.Instance);
            contextSender.SetValue(flagshipContext, sender);

            var flagshipVisitorService = new FlagshipVisitorService(flagshipContext);
            var decisionManager = flagshipVisitorService.GetType().GetField("decisionManager", System.Reflection.BindingFlags.NonPublic
    | System.Reflection.BindingFlags.Instance);
            decisionManager.SetValue(flagshipVisitorService, mockClient.Object);

            var flagship = new FlagshipClient(flagshipContext);
            var fsVisService = flagship.GetType().GetField("fsVisitorService", System.Reflection.BindingFlags.NonPublic
    | System.Reflection.BindingFlags.Instance);
            fsVisService.SetValue(flagship, flagshipVisitorService);

            var flagshipVisitor = flagship.NewVisitor(visitorId, context);
          
            return flagshipVisitor;
        }
    }
}
