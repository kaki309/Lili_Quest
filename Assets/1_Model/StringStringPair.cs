[System.Serializable]
public class StringStringPair
{
    public string key;
    public string value;

    public StringStringPair() { }

    public StringStringPair(string key, string value)
    {
        this.key = key;
        this.value = value;
    }
}
