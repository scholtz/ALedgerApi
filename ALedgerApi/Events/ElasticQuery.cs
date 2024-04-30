using Elastic.Clients.Elasticsearch.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ALedgerApi.Events
{
    public class ElasticQuery
    {
        public int? Limit { get; set; }
        public int? Offset {  get; set; }      
    }

    [JsonConverter(typeof(ElasticQueryDataConverter))]
    public class ElasticMatch
    {
        public string QueryPropertyName { get; set; }
        public QueryProperty QueryProperty { get; set; }
    }

    public class QueryProperty
    {
        [JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
        public string Query { get; set; }        
    }

    public class ElasticMust
    {
        [JsonProperty("match", NullValueHandling = NullValueHandling.Ignore)]
        public ElasticMatch Match { get; set; }        
    }

    public class ElasticQueryDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(object);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type type = value.GetType();

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            string dataPropertyName = (string)type.GetProperty("QueryPropertyName", bindingFlags).GetValue(value);
            if (string.IsNullOrEmpty(dataPropertyName))
            {
                dataPropertyName = "Data";
            }
            var property = type.GetProperty("QueryProperty");

            JObject jo = new JObject();
            if (property.PropertyType == typeof(string) ||
                property.PropertyType == typeof(bool) ||
                property.PropertyType == typeof(int) ||
                property.PropertyType == typeof(decimal) ||
                property.PropertyType == typeof(double) ||
                property.PropertyType == typeof(long) ||
                property.PropertyType == typeof(DateTimeOffset) ||
                property.PropertyType == typeof(DateTime))
            {
                jo.Add(dataPropertyName, new JValue(property.GetValue(value)));
            }
            else if (property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null)
            {
                jo.Add(dataPropertyName, JArray.FromObject(property.GetValue(value)));
            }
            else
            {
                jo.Add(dataPropertyName, JObject.FromObject(property.GetValue(value)));
            }
            //jo.Add(dataPropertyName, JObject.FromObject(type.GetProperty("QueryProperty").GetValue(value)));// JArray.FromObject(type.GetProperty("QueryProperty").GetValue(value)));
            foreach (PropertyInfo prop in type.GetProperties().Where(p => !p.Name.StartsWith("QueryProperty")))
            {
                jo.Add(prop.Name, new JValue(prop.GetValue(value)));
            }
            jo.WriteTo(writer);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
