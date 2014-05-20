namespace IoFx.Connections
{
    /// <summary>
    /// A composer is used to bridge a pair of IoChannels
    /// Components like dispatchers basically consist of layered
    /// composers which have a producer uplink and and consumer downlink
    /// The producers push into the layres as requests and the layer below 
    /// subscribe for responses from the composer
    /// </summary>
    /// <typeparam name="TOutputs"></typeparam>
    /// <typeparam name="TInputs"></typeparam>
    public class Composer<TOutputs, TInputs>
    {
        public Composer(IConnection<TOutputs> requests, IConnection<TInputs> responses)
        {
            this.Outputs = requests;
            this.Inputs = responses;
        }

        public IConnection<TOutputs> Outputs { get; private set; }

        public IConnection<TInputs> Inputs { get; private set; }
    }
}
