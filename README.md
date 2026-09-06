# MMNextPOS Custom Agent Setup

ဒီ package က `MMNextPOS` အတွက် **approval-based custom agent workflow** ကို တည်ဆောက်ပေးပါတယ်။ Agent က request ကို အရင်ခွဲခြမ်းပြီး Domain, Infrastructure, Application, WinForms နဲ့ Tests အလိုက် task ခွဲပေးမယ်။ NVIDIA-compatible API ကို advisory analysis အတွက်သာ အသုံးပြုပြီး၊ code ပြင်ခြင်း သို့မဟုတ် command run ခြင်းမပြုမီ user approval လိုအပ်အောင် သတ်မှတ်ထားပါတယ်။

## ပါဝင်သည့်ဖိုင်များ

| ဖိုင် | ရည်ရွယ်ချက် |
|---|---|
| `.github/agents/mmnextpos-orchestrator.agent.md` | Main orchestrator; plan, approval gate, implementation flow, review handoff |
| `.github/agents/mmnextpos-planner.agent.md` | Read-only implementation planner |
| `.github/agents/mmnextpos-reviewer.agent.md` | Read-only diff, security, architecture, and test reviewer |
| `tools/nvidia_router.py` | NVIDIA OpenAI-compatible endpoint ကို သုံးပြီး advisory sub-task များ ခွဲဝေစစ်ဆေးသည့် script |
| `tools/Invoke-MMNextPOSRouter.ps1` | Windows PowerShell launcher |
| `config/nvidia.env.example` | Secret မပါသော environment variable template |
| `Install-MMNextPOSAgent.ps1` | Project ထဲသို့ setup files ကူးထည့်သည့် installer |

## Windows တွင် တပ်ဆင်ခြင်း

ZIP ကို extract လုပ်ပြီး PowerShell ကို **ပုံမှန် user အဖြစ်** ဖွင့်ပါ။ Admin rights မလိုအပ်ပါက မသုံးပါနှင့်။ ထို့နောက်—

```powershell
Set-ExecutionPolicy -Scope Process Bypass
cd C:\path\to\MMNextPOS-agent-setup
.\Install-MMNextPOSAgent.ps1 -ProjectRoot 'J:\Project 1\MMNext POS'
```

Installer က existing source code ကို မပြင်ပါ။ `.github/agents` ထဲတွင် agent profiles၊ `tools\mmnextpos-agent` ထဲတွင် router files၊ `config` ထဲတွင် secret-free template ကိုသာ ထည့်ပေးပါမယ်။

## NVIDIA API သတ်မှတ်ခြင်း

NVIDIA account မှရရှိသော key ကို source code ထဲ မရေးဘဲ Windows user environment variable အဖြစ်သတ်မှတ်ပါ။

```powershell
[Environment]::SetEnvironmentVariable('NVIDIA_API_KEY', 'YOUR_KEY_HERE', 'User')
```

Terminal အသစ်ဖွင့်ပြီးမှ router ကို စမ်းပါ။ Model IDs နှင့် availability က account၊ quota၊ region နဲ့ အချိန်အလိုက် ပြောင်းလဲနိုင်သောကြောင့် `NVIDIA_MODEL_*` values တွေကို hard-code မလုပ်ထားပါ။ Router က `/v1/models` ကို စစ်ပြီး configured model သို့မဟုတ် compatible name ကို ရွေးပါမယ်။ NVIDIA access ကို **free ဖြစ်သည်ဟု မယူဆပါနှင့်**—သင့် account ထဲတွင် quota/terms ကို ကိုယ်တိုင်စစ်ပါ။

ဥပမာ advisory run—

```powershell
cd 'J:\Project 1\MMNext POS'
.\tools\mmnextpos-agent\Invoke-MMNextPOSRouter.ps1 `
  -Task 'Add barcode scanning validation to the sales workflow' `
  -Context 'Relevant redacted file names and small excerpts only'
```

Router သည် analysis JSON ကို stdout သို့ထုတ်ပြီး source files မပြင်၊ build မလုပ်၊ test မလုပ်၊ database မပြင်ပါ။ API key မရှိလျှင် ရပ်တန့်ပါမယ်။

## Copilot အသုံးပြုခြင်း

Repository ကို VS Code/Copilot ဖြင့်ဖွင့်ပြီး custom agent selector ထဲမှ `mmnextpos-orchestrator` ကိုရွေးပါ။ ပထမ prompt တွင် requirement ပေးပါ။ Agent က **Understanding → Affected areas → Task breakdown → NVIDIA advisory routing → Proposed changes → Verification plan → Approval required** အစီအစဉ်နဲ့ ပြန်မေးပါမယ်။ `Approve this plan?` ကို သင်အတည်ပြုပြီးမှသာ edit/build/test ဆက်လုပ်ခွင့် ရှိပါတယ်။ Planner နဲ့ Reviewer ကို handoff buttons ဖြင့် သီးခြားသုံးနိုင်ပါတယ်။

## လုံခြုံရေးနှင့် ကန့်သတ်ချက်များ

ဒီ setup က Copilot custom-agent profiles နဲ့ local advisory router ဖြစ်ပါတယ်။ `github.exe` ကို package ထဲသို့ မကူးပါ၊ executable upload မလုပ်ပါ၊ Windows မှာရှိပြီးသား Copilot installation ကိုပဲ အသုံးပြုပါမယ်။ NVIDIA router သည် Copilot ကိုယ်တိုင်၏ model backend ကို အလိုအလျောက်ပြောင်းပေးခြင်းမဟုတ်ဘဲ planning/review အတွက် သီးခြား advisory calls ပြုလုပ်တာဖြစ်ပါတယ်။ Copilot model selection နှင့် NVIDIA API model routing သည် သီးခြားအရာများဖြစ်သည်။

Agent သည် API key၊ connection string password၊ user data၊ database dump သို့မဟုတ် repository အပြည့်အစုံကို ပြင်ပ model သို့ မပို့ရပါ။ Database migration, destructive command, release publish, force-push နှင့် unrelated refactor များအတွက် သီးခြား approval လိုအပ်ပါမယ်။

## Verification checklist

တပ်ဆင်ပြီးနောက် agent file များကို review လုပ်ပါ။ ထို့နောက် သီးခြား branch တစ်ခုတွင် small planning request ဖြင့်စမ်းပါ။ Plan ကို approve မလုပ်ဘဲ file changes မဖြစ်ကြောင်း စစ်ပါ။ Approve ပြီးနောက် relevant unit tests နှင့် `dotnet build -c Release` ကို run ခိုင်းပြီး reviewer handoff ဖြင့် diff ကိုပြန်စစ်ပါ။ Integration tests အတွက် Docker/MySQL အခြေအနေကို အရင်အတည်ပြုပါ။

## အခြေခံ project context

MMNextPOS သည် .NET 8 WinForms + DevExpress၊ Domain/Infrastructure/Application/Presentation layered architecture၊ Dapper/MySQL၊ xUnit နှင့် Testcontainers ကို အသုံးပြုထားပါတယ်။ Repository ရှိ `.github/AGENTS.md` နှင့် `.claude/AGENTS.md` ညွှန်ကြားချက်များကို agent profiles များက လိုက်နာရန် သတ်မှတ်ထားပါတယ်။

## References

1. [GitHub: Creating custom agents for Copilot cloud agent](https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/customize-cloud-agent/create-custom-agents)
2. [Visual Studio Code: Custom agents](https://code.visualstudio.com/docs/agent-customization/custom-agents)
3. [NVIDIA API Documentation](https://docs.api.nvidia.com/)
