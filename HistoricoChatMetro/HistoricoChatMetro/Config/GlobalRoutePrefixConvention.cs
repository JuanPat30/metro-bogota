namespace HistoricoChatMetro.Config
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Routing;

    public class GlobalRoutePrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _routePrefix;

        public GlobalRoutePrefixConvention(IRouteTemplateProvider routeTemplateProvider)
        {
            _routePrefix = new AttributeRouteModel(routeTemplateProvider);
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var selector in controller.Selectors.Where(s => s.AttributeRouteModel != null))
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        _routePrefix,
                        selector.AttributeRouteModel);
                }

                // If the controller has no route attributes, add the prefix
                foreach (var selector in controller.Selectors.Where(s => s.AttributeRouteModel == null))
                {
                    selector.AttributeRouteModel = _routePrefix;
                }
            }
        }
    }
}
