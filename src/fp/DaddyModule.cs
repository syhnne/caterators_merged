using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caterators_by_syhnne.fp;

// 现在是2024年8月1日晚上17点51分，我在此隆重宣布：写完这玩意儿我就开摆
// 他妈的，两个月前说这个东西只是想口嗨来着，怎么就写起来了

public class DaddyModule
{
    public WeakReference<Player> owner;
    public Player player { get {
            if (owner.TryGetTarget(out Player player)) { return player; }
            return null;
    }}




}
