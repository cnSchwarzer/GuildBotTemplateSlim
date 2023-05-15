namespace GuildBotTemplate.Utils; 

public static class TypeExtension {
    public static T ToSubClass<T>(this object o) where T: new() {
        var to = o.GetType();
        var td = typeof(T);

        if (!td.IsSubclassOf(to))
            throw new InvalidCastException();
        
        var properties = to.GetProperties();
        var ret = new T();
        
        properties.ToList().ForEach(property =>
        {
            //Check whether that property is present in derived class
            var isPresent = td.GetProperty(property.Name);
            if (isPresent != null && property.CanWrite)
            {
                //If present get the value and map it
                var value = property.GetValue(o);
                property.SetValue(ret, value);
            }
        });

        return ret;
    }

    public static string SignNumber(this int val) {
        return val > 0 ? "+" + val : val.ToString();
    } 
    
    
}