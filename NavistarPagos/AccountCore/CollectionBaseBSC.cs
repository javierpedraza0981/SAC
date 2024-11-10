// Autor:   Juan Carlos Martinez Gonzalez
//
using System;
using System.Collections;

namespace PagoEnLinea
{
    [Serializable]
    public class ColeccionBaseBSC : CollectionBase
    {
        private string nameClase = "";
        public string NameClase
        {
            get { return nameClase; }
            set { nameClase = value; }
        }
        public ICollection ObtenerDatos()
        {
            return this.List;
        }
        /// <summary>
        /// Obtenemos el numero de elementos que existen en la coleccion
        /// </summary>
        public int NumeroElementos
        {
            get
            {
                return this.List.Count;
            }
        }
        #region Events Implementation
        protected override void OnInsert(int index, Object value)
        {
            if (value.GetType() != Type.GetType(NameClase))
                throw new ArgumentException("El valor debe ser de tipo " + NameClase, "valor");
        }

        protected override void OnRemove(int index, Object value)
        {
            if (value.GetType() != Type.GetType(NameClase))
                throw new ArgumentException("El valor debe ser de tipo " + NameClase, "valor");
        }

        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            if (newValue.GetType() != Type.GetType(NameClase))
                throw new ArgumentException("El valor debe ser de tipo " + NameClase, "nuevoValor");
        }

        protected override void OnValidate(Object value)
        {
            if (value.GetType() != Type.GetType(NameClase))
                throw new ArgumentException("El valor debe ser de tipo " + NameClase);
        }
        #endregion
    }
}
