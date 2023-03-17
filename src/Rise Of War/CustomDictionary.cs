namespace RiseOfWar
{
    /*
    [SerializeField]
    public class CustomDictionary<T,T2>
    {
        public List<ValueSet> Values = new List<ValueSet>();

        public CustomDictionary()
        {
            Values = new List<ValueSet>();
        }

        public T GetValueFromKey(T2 _key)
        {
            for (int _i = 0; _i < Values.Count; ++_i)
            {
                if (Values[_i].key.Equals(_key))
                {
                    return Values[_i].value;
                }
            }

            return default(T);
        }

        public T2 GetKeyFromValue(T _value)
        {
            for (int _i = 0; _i < Values.Count; ++_i)
            {
                if (Values[_i].value.Equals(_value))
                {
                    return Values[_i].key;
                }
            }

            return default(T2);
        }

        [SerializeField]
        public class ValueSet
        {
            public T value;
            public T2 key;

            public ValueSet(T _value, T2 _key)
            {
                value = _value;
                key = _key;
            }
        }
    }
    */
}