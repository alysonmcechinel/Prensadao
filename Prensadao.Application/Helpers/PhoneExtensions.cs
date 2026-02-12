namespace Prensadao.Application.Helpers;

public static class PhoneExtensions
{
    public static void ValidatePhone(this string phone)
    {
        phone = phone.Trim();
        if (!(phone.Length == 10 || phone.Length == 11))
        {
            throw new ArgumentException("Número de telefone inválido.");
        }
    }
}
