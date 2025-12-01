namespace TDPG.EffectSystem.ElementLogic
{

    public enum EffectParameter
    {
        Duration,            // Czas trwania efektu (dotyczy np. "SlowDown", "Infekcja")A
        HealthChange,        // Zmiana zdrowia (dotyczy "HealthDown", "Heal", "Egzekucja", "Infekcja")A
        SlowdownFactor,      // Wspó³czynnik spowolnienia (dotyczy "SlowDown")A
        SlowdownOverTime,    // Spowolnienie z czasemA
        HealthDrain,         // Stopniowe obni¿anie zdrowia przeciwnika (dotyczy "Zatrucie")A
        Scaling,             // Zmiana skali przy transformacji obiektu (dotyczy "Zmiana wielkoœci pocisku")A
        StunDuration,        // Czas og³uszenia (dotyczy "Og³uszenie")A
    }

}