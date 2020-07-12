using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using MvcThrottle;

namespace testMvcThrottle
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            bool.TryParse(ConfigurationManager.AppSettings["IpRateLimitEnable"], out var ipRateLimitEnable);

            if (ipRateLimitEnable)
            {
                var throttleFilter = new MvcThrottleCustomFilter
                {
                    Policy = new ThrottlePolicy(2, 30)
                    {
                        IpThrottling = true,
                        StackBlockedRequests = true,
                    },
                    Repository = new CacheRepository(),
                    QuotaExceededResponseCode = HttpStatusCode.OK,
                    Logger = new MvcThrottleCustomLog()
                };

                filters.Add(throttleFilter);
            }
        }
    }

    public class MvcThrottleCustomLog : IThrottleLogger
    {
        public void Log(ThrottleLogEntry entry)
        {
            Debug.WriteLine("{0} Request {1} from {2} has been blocked, quota {3}/{4} exceeded by {5}",
                            entry.LogDate, entry.RequestId, entry.ClientIp, entry.RateLimit, entry.RateLimitPeriod, entry.TotalRequests);        }
    }

    public class MvcThrottleCustomFilter : ThrottlingFilter
    {
        protected override ActionResult QuotaExceededResult(RequestContext filterContext, string message, HttpStatusCode responseCode, string requestId)
        {
            return new JsonResult
            {
                Data = new
                {
                    Error = "系統很忙 !!"
                },
               JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}