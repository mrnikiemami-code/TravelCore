/** Presentation views for ReferenceData Admin read/picker (not domain SoT). */

export type CountryCatalogView = {
  alpha2Code: string;
  alpha3Code: string;
  numericCode: string | null;
  englishName: string;
};

export type CurrencyCatalogView = {
  code: string;
  englishName: string;
  minorUnits: number;
  symbol: string | null;
};

export type LocaleCatalogView = {
  code: string;
  englishName: string;
};
