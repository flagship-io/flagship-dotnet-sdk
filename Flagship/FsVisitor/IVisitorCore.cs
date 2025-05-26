using System.Collections.Generic;
using System.Threading.Tasks;
using Flagship.Hit;

namespace Flagship.FsVisitor
{
    public interface IVisitorCore
    {
        /// <summary>
        /// Update the visitor context values, matching the given keys, used for targeting.
        /// </summary>
        /// <param name="context">A Set of keys, values.</param>
        void UpdateContext(IDictionary<string, object> context);

        /// <summary>
        /// Update the visitor context values, matching the given key or created one if there is no previous matching value
        /// </summary>
        /// <param name="key">Context key.</param>
        /// <param name="value">Context value.</param>
        void UpdateContext(string key, string value);

        /// <summary>
        /// Update the visitor context values, matching the given key or created one if there is no previous matching value
        /// </summary>
        /// <param name="key">Context key.</param>
        /// <param name="value">Context value.</param>
        void UpdateContext(string key, double value);

        /// <summary>
        /// Update the visitor context values, matching the given key or created one if there is no previous matching value
        /// </summary>
        /// <param name="key">Context key.</param>
        /// <param name="value">Context value.</param>
        void UpdateContext(string key, bool value);

        /// <summary>
        /// Clear the actual visitor context
        /// </summary>
        void ClearContext();

        /// <summary>
        /// In DecisionApi Mode this function calls the Flagship Decision API to run campaign assignments according to the current user context and retrieve applicable flags.
        /// In bucketing Mode, it checks bucketing file, validates campaigns targeting the visitor, assigns a variation, and retrieves applicable flags
        /// </summary>
        /// <returns></returns>
        Task FetchFlags();

        /// <summary>
        /// Send Hit to Flagship servers for reporting.
        /// </summary>
        /// <param name="hit">Hit to send.</param>
        /// <returns></returns>
        Task SendHit(HitAbstract hit);

        /// <summary>
        /// Authenticate anonymous visitor
        /// </summary>
        /// <param name="visitorId">Id of the new authenticated visitor.</param>
        void Authenticate(string visitorId);

        /// <summary>
        /// This function change authenticated Visitor to anonymous visitor
        /// </summary>
        void Unauthenticate();
    }
}
