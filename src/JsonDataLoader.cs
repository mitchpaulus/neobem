using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace src
{
    public class JsonDataLoader
    {
        public JsonDataLoader()
        {
        }

        public Expression Load(string json)
        {
            object myObj = JsonConvert.DeserializeObject(json);
            return LoadJSONValue((JToken)myObj);
        }

        private IdfPlusObjectExpression LoadJSONObject(JObject jsonObject)
        {
            IdfPlusObjectExpression objectExpression = new();

            objectExpression.Members = jsonObject.Properties()
                .ToDictionary(property => property.Name,
                    property => LoadJSONValue(property.Value));

            return objectExpression;
        }

        private Expression LoadJSONValue(JToken jToken)
        {
            // There is a generally close 1:1 mapping between JSON types and Neobem internal types.
            if (jToken is JObject jObject) return LoadJSONObject(jObject);
            if (jToken is JArray jArray)   return LoadJSONArray(jArray);
            if (jToken is JValue jValue)
            {
                if (jValue.Type == JTokenType.Boolean) return new BooleanExpression((bool) jValue.Value);
                if (jValue.Type == JTokenType.String) return new StringExpression((string) jValue.Value);
                if (jValue.Type == JTokenType.Integer || jValue.Type == JTokenType.Float) return new NumericExpression(Convert.ToDouble(jValue.Value));
                // Need to make decision about how to handle null values in JSON.
                if (jValue.Type == JTokenType.Null) return new StringExpression("null");
            }

            throw new NotImplementedException();
        }

        private ListExpression LoadJSONArray(JArray jArray) => new(jArray.Select(LoadJSONValue).ToList());
    }
}