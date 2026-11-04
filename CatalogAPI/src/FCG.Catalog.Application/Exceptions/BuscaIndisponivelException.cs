namespace FCG.Catalog.Application.Exceptions;

/// <summary>O motor de busca (OpenSearch) não respondeu ou recusou a operação.</summary>
public class BuscaIndisponivelException : Exception
{
    public BuscaIndisponivelException(string mensagem, Exception? inner = null) : base(mensagem, inner) { }
}
