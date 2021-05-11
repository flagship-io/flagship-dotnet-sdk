using Flagship.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Services.Decision
{
    public interface IDecisionManager
    {
        Task<DecisionResponse> GetResponse(DecisionRequest request);
    }
}
