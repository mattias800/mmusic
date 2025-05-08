namespace MusicGQL.Types;

public class Subscription
{
    [Subscribe]
    public Ping Ping([EventMessage] Ping ping) => ping;
}
