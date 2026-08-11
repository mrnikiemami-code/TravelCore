# Page Archetype Registry

منبع: [`../architecture/13-reference-page-archetypes.md`](../architecture/13-reference-page-archetypes.md)

فازهای پیاده‌سازی تقریبی از ROADMAP؛ فاز جدید اختراع نشده.

| Archetype | Composition Root / Primary | Primary Intent | SEO Role (default direction) | Complexity | Expected Implementation Phase |
|-----------|----------------------------|----------------|------------------------------|------------|-------------------------------|
| ForeignTourDetailPage | Tour (+ Place, Pricing, SEO, Media) | Suitability + commercial action | Primary Indexable Resource | High | P14 after Tour/Pricing foundations |
| ExperienceTourDetailPage | Tour (+ Destination/Place, Media, SEO) | Understand experience fit | Primary Indexable Resource | High | P14 after Experience Tour |
| TourListingPage | Tour (+ Search/SEO as applicable) | Discover/narrow tours | Normally utility / Potential Indexable | Medium | P14 public Tour experience |
| DestinationLandingPage | Destination (+ Tour, Place, Content, UGC, SEO) | Understand destination + discover options | Primary Indexable Resource | High | Destination foundation P04/P05; public later |
| HotelDetailPage (catalog) | Place.Hotel (+ Destination, Tours, UGC, SEO; HotelBooking optional CTA) | Understand hotel place | Potential/Primary Indexable | Medium | P07 Place Catalog |
| RestaurantDetailPage | Place.Restaurant | Relevance during travel | Potential Indexable | Medium | P07 |
| AttractionDetailPage | Place.Attraction | Plan visit | Potential Indexable | Medium | P07 |
| ArticlePage | Content | Informational guidance | Primary Indexable Resource | Medium | P08 Content |
| TraveloguePage | UGC | Story/community narrative | Potential Indexable (quality-gated) | Medium | P16 UGC |
| VisaDetailPage | Visa (+ Destination/Content) | Understand visa + next step | Potential/Primary Indexable | Medium | P17 Visa |
| SearchResultsPage | Search (+ composed result types) | Find across catalog | Normally NoIndex Utility | Medium | P15 Search |
| SeoLandingPage | SEO landing definition (+ composed modules) | Intentful controlled landing | Controlled SEO Landing | Medium | P05/P14/P26 per landing approval |

**صریح:** ردیف Composition Root به‌معنای مالکیت همهٔ دادهٔ ترکیب‌شده نیست.

جزئیات هر archetype در فایل‌های `01`–`08`.

### Reference mapping (from page-registry)

| Registry ID | Observed pattern (useful) | TravelCore interpretation | Must NOT copy |
|-------------|---------------------------|---------------------------|---------------|
| REF-LS-001 | Foreign package structure: flight + hotel options + multi-component price | Informs ForeignTourDetailPage anatomy | Brand, text, assets, exact UI |
| REF-LS-002 | Structured itinerary vs single text blob | Informs ExperienceTourDetailPage | Brand, text, assets, exact UI |
| REF-LS-003 | Destination-centric discovery + internal linking | Informs DestinationLanding + SEO linking | Visual homepage / proprietary IA copy |
| REF-TH-001 | Operational commerce breadth (flight/hotel/tour/visa) | Informs product coverage; not page layouts | B2B flows UI, branding, code |

Where registry lacks detail for a specific archetype (e.g. Visa page wireframe): **Reference evidence incomplete** — TravelCore archetype still defined from domain/architecture.
