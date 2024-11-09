using Enums;
using Factories;

namespace Tests;

public abstract class AdjudicationTestBase
{
    protected MapFactory MapFactory { get; private set; }
    protected DefaultWorldFactory DefaultWorldFactory { get; private set; }

    protected const Nation nation1 = Nation.Ankara;
    protected const Nation nation2 = Nation.Rome;
    protected const Nation nation3 = Nation.Greece;
    protected const Nation nation4 = Nation.Serbia;
    protected const Nation nation5 = Nation.Munich;
    protected const Nation nation6 = Nation.Sweden;
    protected const Nation nation7 = Nation.London;

    public AdjudicationTestBase()
    {
        MapFactory = new();
        DefaultWorldFactory = new();
    }
}
