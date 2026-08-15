/**
 * Frontend presentation contract for imagery.
 * Media Domain Model ≠ API Contract ≠ Page View Model
 *
 * Does not represent MediaAsset persistence / provider metadata.
 */
export type MediaImagePresentation = {
  /** Approved public or static URL/path for presentation. */
  src: string;
  /**
   * Informative images: meaningful alternative text from the presentation model.
   * Decorative images: empty string `""` (explicit semantic intent).
   * Never derive alt from filename/URL automatically.
   */
  alt: string;
  /** Intrinsic pixel width when known (intrinsic layout). */
  width?: number;
  /** Intrinsic pixel height when known (intrinsic layout). */
  height?: number;
  /**
   * CSS aspect-ratio for fill/responsive wrappers (e.g. `"16 / 9"`).
   * Stabilizes layout to reduce CLS when using `fill`.
   */
  aspectRatio?: `${number} / ${number}` | string;
  /**
   * Responsive `sizes` hint for next/image fill/responsive paths.
   * Caller supplies intent (hero vs card); defaults are conservative.
   */
  sizes?: string;
  /**
   * LCP/critical media only — caller-explicit.
   * Default behavior is non-priority (lazy/default loading).
   */
  priority?: boolean;
};
