using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel;

public class SoapActionBehavior : IEndpointBehavior
{
    private readonly string _soapAction;

    public SoapActionBehavior(string soapAction)
    {
        _soapAction = soapAction;
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(new SoapActionMessageInspector(_soapAction));
    }
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}

public class SoapActionMessageInspector : IClientMessageInspector
{
    private readonly string _soapAction;

    public SoapActionMessageInspector(string soapAction)
    {
        _soapAction = soapAction;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState) { }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        HttpRequestMessageProperty httpRequest;
        if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
        {
            httpRequest = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
        }
        else
        {
            httpRequest = new HttpRequestMessageProperty();
            request.Properties.Add(HttpRequestMessageProperty.Name, httpRequest);
        }
        httpRequest.Headers["SOAPAction"] = _soapAction;
        return null;
    }
}