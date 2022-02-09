using NBasis.OneTable;

namespace NBasis.OneTableTests.Unit
{

    public class TestContext : TableContext
    {
        public TestContext()
        {
            Configuration = TableConfiguration.Default();
            AttributizerSettings = NBasis.OneTable.Attributization.AttributizerSettings.Default();
        }
    }
}
