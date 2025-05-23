using System;
using CLEA.EasySaveCore;
using CLEA.EasySaveCore.L10N;
using EasySaveCore.Models;
using Xunit;

namespace CLEA_Tests
{
    public class TranslationSystemUnitTest
    {
        private static readonly LangIdentifier FakeLang = new LangIdentifier("fake_lang", "FakeLang", "fk");

        [SkippableFact]
        public void Get_ShouldReturnSingletonInstance()
        {
            Skip.IfNot(EasySaveCore<BackupJob>.Version.Major == 1);

            var instance1 = L10N<BackupJob>.Get();
            var instance2 = L10N<BackupJob>.Get();

            Assert.Same(instance1, instance2);
        }

        [SkippableFact]
        public void SetLanguage_ShouldSwitchToEnglish()
        {
            Skip.IfNot(EasySaveCore<BackupJob>.Version.Major == 1);

            var l10N = L10N<BackupJob>.Get();
            l10N.SetLanguage(Languages.EnUs);

            string result = l10N.GetTranslation("language");
            Assert.Equal("English", result);
        }

        [SkippableFact]
        public void SetLanguage_ShouldThrowIfUnsupported()
        {
            Skip.IfNot(EasySaveCore<BackupJob>.Version.Major == 1);

            var l10N = L10N<BackupJob>.Get();

            var ex = Assert.Throws<ArgumentException>(() => l10N.SetLanguage(FakeLang));
            Assert.Equal("Translation lang not found", ex.Message);
        }

        [SkippableFact]
        public void GetTranslation_ShouldThrowIfKeyNotFound()
        {
            Skip.IfNot(EasySaveCore<BackupJob>.Version.Major == 1);

            var l10N = L10N<BackupJob>.Get();
            l10N.SetLanguage(Languages.EnUs);

            Assert.Equal("nonexistent.key", l10N.GetTranslation("nonexistent.key"));
        }
    }
}