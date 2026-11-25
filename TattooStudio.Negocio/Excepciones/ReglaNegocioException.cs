namespace TattooStudio.Negocio.Excepciones;

public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string message) : base(message)
    {
    }
}
