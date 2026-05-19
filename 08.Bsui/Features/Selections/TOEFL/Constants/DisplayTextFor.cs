using Psmp.Shared.Common.Extensions;

namespace Psmp.Bsui.Features.Selections.TOEFL.Constants;

public static class DisplayTextFor
{
    public const string TOEFL = nameof(TOEFL);
    public static readonly string TOEFLCreate = nameof(TOEFLCreate).SplitWords();
    public static readonly string TOEFLDetail = nameof(TOEFLDetail).SplitWords();
    public static readonly string TOEFLUpdate = nameof(TOEFLUpdate).SplitWords();
}
