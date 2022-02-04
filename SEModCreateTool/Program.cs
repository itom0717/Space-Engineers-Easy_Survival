
string srcPath = @"D:\Steam\steamapps\common\SpaceEngineers\Content\Data";
#if DEBUG
string dstPath = @"C:\Users\itom\OneDrive\Source\repos\Space Engineers\TEST";
#else
string dstPath = @"C:\Users\itom\OneDrive\Source\repos\Space Engineers\Space-Engineers-Easy_Survival";
#endif

var easySurvival = new SEModCreateTool.EasySurvival();
easySurvival.Create(srcPath, dstPath);

