using System;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using YelpSharp;
using YelpSharp.Data;

namespace CRMWebAPIYelpExample.Controllers
{
    public class AccountEnrichmentController : ApiController
    {
        private static string _connection;
        private OrganizationService _orgService;

        public void Get()
        {
            _connection = ConfigurationManager.ConnectionStrings["CRMOnlineO365"].ConnectionString;
            CrmConnection connection = CrmConnection.Parse(_connection);

            EntityCollection results = GetYelpAccounts();
            if (!results.Entities.Any()) return;

            using (_orgService = new OrganizationService(connection))
            {
                foreach (Entity entity in results.Entities)
                {
                    GetYelpData(entity.Id, entity.GetAttributeValue<string>("test9_yelpid"));
                }
            }
        }

        private void GetYelpData(Guid id, string yelpId)
        {
            var options = new Options
            {
                AccessToken = ConfigurationManager.AppSettings["YelpAPIToken"],
                AccessTokenSecret = ConfigurationManager.AppSettings["YelpAPITokenSecret"],
                ConsumerKey = ConfigurationManager.AppSettings["YelpAPIConsumerKey"],
                ConsumerSecret = ConfigurationManager.AppSettings["YelpAPIConsumerSecret"]
            };

            var yelp = new Yelp(options);
            Business business = yelp.GetBusiness(yelpId).Result;
            if (business != null)
                UpdateAccount(id, business.rating);
        }

        private EntityCollection GetYelpAccounts()
        {
            CrmConnection connection = CrmConnection.Parse(_connection);
            using (_orgService = new OrganizationService(connection))
            {
                var query = new QueryExpression
                {
                    EntityName = "account",
                    ColumnSet = new ColumnSet("test9_yelpid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = "test9_yelpid",
                                Operator = ConditionOperator.NotNull
                            }
                        }
                    }
                };

                return _orgService.RetrieveMultiple(query);
            }
        }

        private void UpdateAccount(Guid id, double rating)
        {
            var account = new Entity("account") { Id = id };
            account["test9_yelprating"] = rating;

            CrmConnection connection = CrmConnection.Parse(_connection);
            using (_orgService = new OrganizationService(connection))
            {
                _orgService.Update(account);
            }
        }
    }
}