namespace TiendaMusica.Domain.Models.Result
{
    public class Results<T>
    {
        private List<TiendaMusicaError> _errors = [];
        public T Result { get; set; }

        public Results()
        { 
            _errors = new List<TiendaMusicaError>();
        }

        public List<TiendaMusicaError> Errors 
        {
            get
            { 
              _errors ??= new List<TiendaMusicaError>();
                return _errors;
            }
            set
            {
                _errors = value;
            }
        }

        public bool IsSuccess
        {
            get
            {
                return !Errors.Any();
            }
        }

        public bool HasErrors
        {
            get
            {
                return !IsSuccess;
            }
        }

        public Results<T> AddError(ErrorCode errorCode, string message)
        {
            var error = new TiendaMusicaError(errorCode, message);
            AddError(error);
            return this;
        }

        public Results<T> AddErrors(IEnumerable<TiendaMusicaError> errors)
        {
            if(errors != null)
            {
                Errors.AddRange(errors);
            }

            return this;
        }

        public Results<T> AddError(TiendaMusicaError error)
        {
            Errors.Add(error); 
            return this; 
        }
    }
}
