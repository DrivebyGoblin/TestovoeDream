
public readonly struct IAPProduct
{
    public string Id { get; }
    public string LocalizedPrice { get; }

    public IAPProduct(string id, string localizedPrice)
    {
        Id = id;
        LocalizedPrice = localizedPrice;
    }
}