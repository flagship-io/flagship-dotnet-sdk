using System.Collections.Generic;
using System.Threading.Tasks;
using Flagship.Model;
using Flagship.Model.Hits;

namespace Flagship
{
    public interface IFlagshipVisitor
    {
        void UpdateContext(IDictionary<string, object> context);
        void UpdateContext(string key, object value);
        Task SynchronizeModifications();
        T GetModification<T>(string key, T defaultValue = default, bool activate = true);
        ModificationInfo GetModificationInfo(string key);
        IDictionary<string, FlagInfo> GetAllModifications();
        Task ActivateModification(string key);
        Task SendHit(HitType type, BaseHit hit);
        Task SendHit<T>(T hit) where T : BaseHit;
    }
}
