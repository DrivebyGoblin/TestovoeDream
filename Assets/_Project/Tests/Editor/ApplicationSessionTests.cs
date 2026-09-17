using System;
using NUnit.Framework;

public sealed class ApplicationSessionTests
{
    private static readonly DateTime Start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void FocusAndPause_KeepOriginalExitTime_ResumeOnlyOnce()
    {
        var session = new ApplicationSession();
        session.SetFocus(false, Start);
        session.SetPause(true, Start.AddSeconds(1));
        session.SetPause(true, Start.AddSeconds(2));
        Assert.That(session.SuspendedAt, Is.EqualTo(Start));
        Assert.That(session.SetFocus(true, Start.AddSeconds(10)), Is.Null);
        Assert.That(session.IsSuspended, Is.True);
        Assert.That(session.SetPause(false, Start.AddSeconds(11)), Is.EqualTo(Start));
        Assert.That(session.SetFocus(true, Start.AddSeconds(12)), Is.Null);
        Assert.That(session.SetPause(false, Start.AddSeconds(12)), Is.Null);
        Assert.That(session.ResumeCount, Is.EqualTo(1));
        Assert.That(session.IsSuspended, Is.False);
    }

    [Test]
    public void PauseThenFocus_ReverseCallbackOrder_StillResumesOnce()
    {
        var session = new ApplicationSession();
        session.SetPause(true, Start);
        session.SetFocus(false, Start.AddSeconds(1));
        Assert.That(session.SetPause(false, Start.AddSeconds(10)), Is.Null);
        Assert.That(session.SetFocus(true, Start.AddSeconds(11)), Is.EqualTo(Start));
        Assert.That(session.ResumeCount, Is.EqualTo(1));
    }

    [Test]
    public void PauseWithoutFocusCallbacks_ResumesAndStartsANewInterval()
    {
        var session = new ApplicationSession();
        Assert.That(session.SetPause(false, Start), Is.Null);
        session.SetPause(true, Start);
        Assert.That(session.SetPause(false, Start.AddSeconds(10)), Is.EqualTo(Start));
        session.SetPause(true, Start.AddSeconds(20));
        Assert.That(session.SuspendedAt, Is.EqualTo(Start.AddSeconds(20)));
        Assert.That(session.SetPause(false, Start.AddSeconds(30)), Is.EqualTo(Start.AddSeconds(20)));
        Assert.That(session.ResumeCount, Is.EqualTo(2));
    }

    [Test]
    public void RepeatedResumeCallbacks_DoNotPayOfflineIncomeTwice()
    {
        var session = new ApplicationSession();
        var wallet = new WalletModel(50);
        var offline = new OfflineProgressService();
        session.SetFocus(false, Start);
        session.SetPause(true, Start);
        session.SetPause(false, Start.AddSeconds(10));

        for (int i = 0; i < 3; i++)
        {
            DateTime? exit = session.SetFocus(true, Start.AddSeconds(10));
            if (exit.HasValue)
            {
                wallet.Add(offline.Calculate(exit.Value, Start.AddSeconds(10), 60, 0, 2, 5).EarnedCoins);
            }
        }

        Assert.That(wallet.Balance, Is.EqualTo(100));
    }
}
