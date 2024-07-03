using Flagship.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Tests.Data
{
    public static class CampaignsData
    {
        public static string GetCampaignsJson()
        {
            return @"{'visitorId':'anonymeId','campaigns':[{'id':'c3ev1afkprbg5u3burag','variation':{'id':'c3mrlpveoqt1lkm7tc00','modifications':{'type':'JSON','value':{'array':[3,3,3],'complex':{'carray':[{'cobject':3}]},'object':{'value':8552}}},'reference':false},'variationGroupId':'c3ev1afkprbg5u3burbg'},{'id':'c2nrh1hjg50l9thhu8bg','variation':{'id':'c2nrh1hjg50l9thhu8dg','modifications':{'type':'JSON','value':{'key':'value'}},'reference':false},'variationGroupId':'c2nrh1hjg50l9thhu8cg'},{'id':'c20j8bk3fk9hdphqtd1g','variation':{'id':'c20j8bk3fk9hdphqtd30','modifications':{'type':'HTML','value':{'my_html':'\u003cdiv\u003e\n \u003cp\u003eoriginal\u003c/ p\u003e\n\u003c/ div\u003e','my_text':null}},'reference':true},'variationGroupId':'c20j8bk3fk9hdphqtd2g'}]}";
        }

        public static DecisionResponse? DecisionResponse()
        {
            return JsonConvert.DeserializeObject<DecisionResponse>(GetCampaignsJson());
        }

        public static Collection<FlagDTO> GetFlag()
        {
            return new Collection<FlagDTO>
            {
                new FlagDTO{
                Key = "key",
                Value = "value",
                VariationId = "c3mrlpveoqt1lkm7tc00",
                CampaignId = "c3ev1afkprbg5u3burag",
                VariationGroupId = "c3ev1afkprbg5u3burbg",
                IsReference = true,
                },
                 new FlagDTO{
                Key = "key2",
                Value = null,
                VariationId = "c3mrlpveoqt1lkm7tc10",
                CampaignId = "c3ev1afkprbg5u3burag",
                VariationGroupId = "c3ev1afkprbg5u3bkrbg",
                IsReference = true,
                }
            };
        }
    }
}
