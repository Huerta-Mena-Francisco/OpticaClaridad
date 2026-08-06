namespace OpticaClaridad.Helpers;

public static class SessionHelper
{
    public static bool EstaAutenticado(ISession session)
    {
        return !string.IsNullOrEmpty(session.GetString("UsuarioId"));
    }

    public static bool EsAdministrador(ISession session)
    {
        return session.GetString("EsAdmin") == "True";
    }
}