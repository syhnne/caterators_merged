using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs;

namespace Caterators_by_syhnne.moon.MoonSwarmer;




/*这波是把fisobs用到极致了
    回头再写吧（疲劳
    准备继承Creature类（乐，真成散装机猫了）不这样的话它钻不了管道
    感觉除了替死以外要是还有别的功能会好一点
    另外需要写一个nsh的复活神经元和这个之间的无缝衔接 也就是说moon活着的时候也能被复活 参考SLOracleBehavior.ConvertingSSSwarmer()
    所以说nsh复活队友的时候最好拿着神经元 不然无缝转换就是有缝转换了
    呃啊 那咋整啊 弄一个圣猫同款大狙吗 什么暴力奶妈（？？*/
public class MoonSwarmer : Creature
{

    public moon.SwarmerManager manager;

    public MoonSwarmer(AbstractCreature abstr, World world) : base(abstr, world)
    {

    }




}