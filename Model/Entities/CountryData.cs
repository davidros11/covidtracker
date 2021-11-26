using System;
class CountryData
{
    public int ConfirmedCases { get; set; }
    public int Deaths { get; set; }
    public int Recovered { get; set; }
    public int ActiveCases { get; set; }
    public int NewCasesDaily { get; set; }
    public int NewCasesWeekly { get; set; }
    public int NewCasesMonthly { get; set; }
    public long Population { get; set; }
    public double SickRate { get; set; }
    public double  FatalityRate { get; set; }
    public double  RecoveryRate { get; set; }
}