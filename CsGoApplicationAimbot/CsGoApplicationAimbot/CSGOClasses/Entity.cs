using System.Collections;
using CsGoApplicationAimbot.CSGOClasses.Fields;

namespace CsGoApplicationAimbot.CSGOClasses
{
    /// <summary>
    /// An abstract class meant for managing Entities
    /// Utilizes a hashtable to manage reading data and cache data
    /// </summary>
    public abstract class Entity
    {

        #region PROPERTIES
        public Hashtable Fields { get; private set; }
        public int Address { get; protected set; }
        #endregion

        #region CONSTRUCTORS
        public Entity(int address)
        {
            Address = address;
            Fields = new Hashtable();
            SetupFields();
        }
        public Entity() : this(0)
        { }
        #endregion

        #region METHODS
        public override string ToString()
        {
            return string.Format("[Entity Address={0}]", Address.ToString("X"));
        }
        #endregion

        #region HELPERS
        protected void AddField<T>(string fieldName, int offset, T value = default(T)) where T : struct
        {
            Fields[fieldName] = new Field<T>(offset, value);
        }
        /// <summary>
        /// Returns the value of the given field if the field has read its value before
        /// Makes the field read its value if it did not do so before
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected T ReadFieldProxy<T>(string fieldName) where T : struct
        {
            Field<T> field = (Field<T>)Fields[fieldName];
            if (!field.ValueRead)
                field.ReadValue(Address);
            return field.Value;
        }
        /// <summary>
        /// Copies the fields of one Entity to another one;
        /// Used for copy-constructors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="other"></param>
        protected void CopyFieldsFrom<T>(T other) where T : Entity
        {
            foreach (string key in other.Fields.Keys)
                Fields[key] = other.Fields[key];
        }

        protected virtual void SetupFields()
        { }
        #endregion
    }
}
