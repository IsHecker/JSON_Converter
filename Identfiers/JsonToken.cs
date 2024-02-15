namespace JsonSerialization.Identfiers
{
    internal enum JsonToken
    {
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        Colon,
        Comma,
        Quotation,
        InQuotation,
        Whitespace,
        String,
        Number,
        Boolean,
        Null
    }
}
