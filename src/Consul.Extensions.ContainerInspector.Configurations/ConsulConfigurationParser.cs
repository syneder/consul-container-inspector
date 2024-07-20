using Microsoft.Extensions.Configuration;
using System.Text;

namespace Consul.Extensions.ContainerInspector.Configurations
{
    /// <summary>
    /// Parses and represents the Consul configuration as an <see cref="IDictionary{string, string?}"/>.
    /// </summary>
    public class ConsulConfigurationParser(StreamReader streamReader)
    {
        private char _currentChar = (char)0;
        private char _currentBracket = (char)0;
        private readonly StringBuilder _content = new();

        /// <summary>
        /// Returns an enumeration of tokens obtained after executing the <see cref="Parse"/> method.
        /// </summary>
        public IEnumerable<string> Tokens { get; private set; } = [];

        /// <summary>
        /// Parses the Consul configuration passed to the stream reader and returns it in key-value format.
        /// </summary>
        /// <returns>Consul configuration as an <see cref="IDictionary{string, string?}"/>.</returns>
        public IDictionary<string, string> Parse()
        {
            var contentTokens = GetTokens();
            Tokens = [.. contentTokens];

            var visitedTokens = new Stack<string>();
            var visitedSections = new Stack<string>();
            var data = new Dictionary<string, string>();

            var contentTokensEnumerator = contentTokens.GetEnumerator();
            while (contentTokensEnumerator.MoveNext())
            {
                var currentToken = contentTokensEnumerator.Current;
                if (currentToken.Length == 1)
                {
                    switch (currentToken[0])
                    {
                        case '{':
                            if (visitedTokens.Count > 0)
                            {
                                visitedSections.Push(visitedTokens.Peek());
                            }

                            continue;

                        case '}':
                            if (visitedSections.Count > 0)
                            {
                                visitedTokens.TryPop(out _);
                                visitedSections.Pop();
                            }

                            continue;

                        case '=':
                            if (visitedTokens.TryPop(out var name) && contentTokensEnumerator.MoveNext())
                            {
                                currentToken = contentTokensEnumerator.Current;
                                if (currentToken.Length == 1 && currentToken[0] == '{')
                                {
                                    visitedTokens.Push(name);
                                    visitedSections.Push(name);
                                    continue;
                                }

                                var configurationPath = ConfigurationPath.Combine(visitedSections.Reverse().Append(name));
                                data.TryAdd(configurationPath, currentToken);
                            }

                            continue;
                    }
                }

                visitedTokens.Push(currentToken);
            }

            return data;
        }

        /// <summary>
        /// Separates content from the stream reader into configuration tokens.
        /// </summary>
        /// <returns>The <see cref="List{string}"/>.</returns>
        private List<string> GetTokens()
        {
            var contentTokens = new List<string>();

            while (!streamReader.EndOfStream)
            {
                if ((_currentChar = (char)streamReader.Read()) == 0)
                {
                    continue;
                }

                if (_currentBracket == 0)
                {
                    // If the quotes were not opened and the beginning of the comment is reached,
                    // ignore any characters until the end of the line. Please note that multi-line
                    // comments are not supported.
                    if (_currentChar == '#')
                    {
                        while (!streamReader.EndOfStream)
                        {
                            if ((_currentChar = (char)streamReader.Read()) == '\n')
                            {
                                break;
                            }
                        }

                        continue;
                    }

                    // The equal sign separate the content into tokens in all cases except when the
                    // content is enclosed in quotation marks. A space before or after the equals
                    // sign is optional. The same applies to curly braces.
                    if (_currentChar == '=' || _currentChar == '{' || _currentChar == '}')
                    {
                        if (_content.Length > 0)
                        {
                            contentTokens.Add(TrimContent());
                        }

                        contentTokens.Add(_currentChar + "");
                        continue;
                    }

                    if (_currentChar == '\'' || _currentChar == '"')
                    {
                        if (_content.Length > 0)
                        {
                            contentTokens.Add(TrimContent());
                        }

                        _currentBracket = _currentChar;
                        continue;
                    }

                    if (_currentChar == ' ' || _currentChar == '\t' || _currentChar == '\n' || _currentChar == '\r')
                    {
                        if (_content.Length > 0)
                        {
                            contentTokens.Add(TrimContent());
                        }

                        continue;
                    }
                }
                else if (_currentBracket == _currentChar)
                {
                    contentTokens.Add(TrimContent());

                    _currentBracket = '\0';
                    continue;
                }

                _content.Append(_currentChar);
            }

            return contentTokens;
        }

        /// <summary>
        /// Clears the <see cref="_content"/> and returns the value before clearing.
        /// </summary>
        private string TrimContent()
        {
            var currentContent = _content.ToString();
            _content.Clear();

            return currentContent;
        }
    }
}
