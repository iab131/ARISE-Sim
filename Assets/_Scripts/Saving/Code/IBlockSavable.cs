using System.Collections.Generic;

public interface IBlockSavable
{
    Dictionary<string, string> SaveInputs();
    void LoadInputs(Dictionary<string, string> inputs);
}
