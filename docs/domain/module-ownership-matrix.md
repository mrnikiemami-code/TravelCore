# Module Ownership Matrix — ماتریس مالکیت سریع

مرجع تفصیلی: [`../architecture/04-module-boundaries.md`](../architecture/04-module-boundaries.md)
قوانین وابستگی: [`../architecture/05-dependency-rules.md`](../architecture/05-dependency-rules.md)

قبل از پیاده‌سازی هر مفهوم بپرسید: **چه کسی مالک است؟** و **این ماژول با چه کسی حرف بزند؟**

---

## Ownership Quick Table

| Module | Category | Primary Ownership | May Reference / Consume | Must Not Own | Must Not Depend On |
|--------|----------|-------------------|-------------------------|--------------|--------------------|
| Identity | Foundation | Authentication identity / Account | PartyId association | Party profile, Access taxonomy, Tour/Booking/Payment | Tour, Booking, Payment, Search, SEO, Notification |
| Access | Foundation | Roles, Permissions, authorization evaluation | Identity subject IDs, Party subject IDs | Credentials, Agency/Person profiles, Tour policies, UI as SoR | Tour, Booking, Payment, Search, SEO, Notification |
| Party | Foundation | Person / Organization / Agency business identity | ReferenceData | Credentials, roles, Tour inventory, Booking, Payment, Destination, Hotel catalog | Tour, Booking, Payment, Search, SEO, Notification |
| ReferenceData | Foundation | Stable shared reference catalogs | — | TourStatus, BookingStatus, PaymentStatus, Destination hierarchy, business lifecycles | All business modules |
| Destination | Discovery | Travel destination hierarchy / discovery nodes | ReferenceData, Media | Hotels, Restaurants, Attractions, Tours, Articles, Reviews | Tour, Place aggregates, Content, UGC, Search, SEO, Notification |
| Media | Discovery | MediaAsset metadata / variants / upload lifecycle | — | Tour gallery ordering, Hotel gallery meaning, Article body, Review logic | Business modules |
| Place | Discovery | Hotel / Restaurant / Attraction catalog | Destination, Media, ReferenceData | Live availability, provider rates, reservation, voucher | HotelBooking inventory, Booking, Payment, Search, SEO, Notification |
| Content | Knowledge | Editorial Article/Guide/Landing/Blocks | Media; Destination; Place/Tour/Visa by ID | Tour product, Place catalog, Visa workflow ownership | Tour/Place/Visa domain for core ops cycles; Search/SEO/Notification as hard deps |
| UGC | Knowledge | Review, Rating, Travelogue, moderation | Identity/Party IDs, Media, target IDs | Target entity aggregates | Being required by Place/Tour/Destination core transactions; Search/SEO/Notification as hard deps |
| Tour | Commerce | TourProduct, Departure, itinerary, TourHotelOption, package transport facts | Destination, Place, Party, Media, ReferenceData | Hotel catalog, Quote calc, Booking, Payment, Search index, SEO engine | Pricing DbContext, Booking, Payment, Search, SEO, Notification, Flight live inventory |
| Pricing | Commerce | TourRate, PriceComponent, ExchangeRate, Quote, conversion | TourDeparture identity (Guid) / Tour contracts (logical only); ReferenceData | Tour catalog tables, Booking history, Payment execution | Tour DbContext / Tour EF ownership, Booking, Payment, Search, SEO, Notification; premature Flight/HotelBooking fare ownership |
| AgencyMarketplace | Commerce | Agency commercial relationship (marketplace) | Party identity (logical Guid) | Party identity SoR, TourProduct catalog, Price/Quote, Booking/Payment | Party/Tour/Pricing DbContext, Booking, Payment, Search, SEO, Notification |
| Visa | Commerce | Visa offerings / requirements / workflow concepts | Destination, ReferenceData, Media | Tour ownership of visa product | Hard bidirectional Tour dependency; Search/SEO/Notification as hard deps |
| Booking | Commerce | Reservation/order state, traveler + quote snapshots | Party, Tour contracts, Pricing Quote contracts | Payment execution, live price engine, Tour catalog, Agency profile | Payment DbContext, Pricing DbContext, Tour DbContext, Search, SEO, Notification |
| Payment | Commerce | Payment lifecycle, attempts, provider results, refund foundation | Booking references/contracts, ReferenceData | Booking business lifecycle, price calculation, Tour, Quote generation | Booking DbContext mutation, Tour, Search, SEO |
| HotelBooking | External | Live hotel provider search/book/voucher + mappings | PlaceId (Hotel-kind), ReferenceData, providers | Canonical Hotel catalog | Place ownership takeover; Pricing until explicitly designed; Search/SEO/Notification as hard deps |
| Flight | External | Live flight search/book/provider offers | ReferenceData, providers | Tour package segment ownership | Tour ownership of live inventory; Pricing until explicitly designed; Search/SEO/Notification as hard deps |
| Search | Platform | Search index / projections / facets / autocomplete | Events/contracts from publishers | Authoritative Tour/Destination/Place/Content/UGC/Visa | Being required by core business transactions |
| SEO | Platform | SeoRoute, canonical, hreflang, redirects, IndexPolicy, sitemap mechanics | Publishable contracts/events | Tour/Place business content ownership | Being required by core business transactions |
| Notification | Platform | Delivery orchestration / channels / delivery state | Semantic business events | Business rule ownership of Booking/Tour/Payment | Being required by core business transactions |

---

## Presentation Surfaces

| Surface | Is Domain Module? | May Invoke | Must Not Own |
|---------|-------------------|------------|--------------|
| Public Website | NO | Module application/read contracts | Business rules |
| Admin Panel | NO | Module application capabilities | Copied Tour/Booking/Pricing logic |
| Agency Panel | NO | Allowed module capabilities | Duplicated commerce domain logic |

---

## Consumer → Provider Interaction Table

| Consumer | Provider | Default Interaction | Reason |
|----------|----------|---------------------|--------|
| Access | Identity | B / C | Subject authentication identity |
| Access | Party | B / C | Business subject |
| Identity | Party | B | Optional account↔Party association |
| Party | ReferenceData | B | Foundational references |
| Destination | ReferenceData | B | ISO/geo references |
| Destination | Media | B | Destination assets |
| Place | Destination | B | Location hierarchy |
| Place | Media | B | Catalog media |
| Place | ReferenceData | B | Shared refs |
| Content | Media | B | Editorial media |
| Content | Destination/Place/Tour/Visa | B / C | Semantic links without ownership transfer |
| UGC | Identity/Party | B | Author/subject |
| UGC | Media | B | User media |
| UGC | Destination/Place/Tour | B | Review target IDs |
| Tour | Destination | B | Destination links |
| Tour | Place | B / C | PlaceId validation (Hotel-kind) / option |
| Tour | Party | B | Agency/offer ownership refs |
| Tour | Media | B | TourMedia |
| Tour | ReferenceData | B | Shared refs |
| Pricing | Tour | C / B | Priceable product/departure validation |
| Pricing | ReferenceData | B | Currency refs |
| AgencyMarketplace | Party | B | Logical Agency identity (Guid); Party remains identity SoR |
| Visa | Destination | B | Applicability |
| Booking | Party | B / C | Customer |
| Booking | Tour | C / B | Product/departure facts |
| Booking | Pricing | C | Quote validity / snapshot |
| Payment | Booking | B / C / D | Payment purpose + events |
| HotelBooking | Place | B / C | Canonical Hotel mapping |
| Flight | ReferenceData | B | Airport/carrier refs |
| Search | Business publishers | D / E | Derived index |
| SEO | Business publishers | D / C / E | Route/index mechanics |
| Notification | Business publishers | D | Delivery reactions |
| Public/Admin/Agency | Any approved modules | C / read composition | Presentation only |

Legend: B=ID reference · C=synchronous contract · D=event consumer · E=projection consumer

---

## Critical Distinctions (checklist)

| Distinction | Owner A | Owner B |
|-------------|---------|---------|
| Authenticated who? | Identity | — |
| Business who? | Party | — |
| Allowed to do? | Access | — |
| Hotel what is it? | Place | — |
| Hotel bookable now? | HotelBooking | — |
| TourProduct | Tour | ≠ TourDeparture (also Tour) |
| Package FlightSegment description | Tour | ≠ Flight live inventory |
| Current commercial rate / Quote | Pricing | — |
| Accepted historical commercial facts | Booking snapshot | — |
| Money movement | Payment | — |
| SEO mechanics | SEO | ≠ Tour content ownership |
| Searchable projection | Search | ≠ Destination/Tour SoR |

---

## Who Owns This Concept? (lookup)

| Concept | Owner |
|---------|-------|
| Account / login | Identity |
| Role / Permission | Access |
| Person / Organization / Agency profile | Party |
| Currency code catalog | ReferenceData |
| Istanbul destination node | Destination |
| MediaAsset | Media |
| Hotel descriptive profile | Place |
| Article / Guide | Content |
| Review / Travelogue | UGC |
| TourProduct / Experience specialization / Itinerary / ExperienceGuideAssignment | Tour |
| TourDeparture / TourHotelOption / FlightSegment | Tour (P11+: Departure scaffolding started; Flight/HotelOption later) |
| TourRate / PriceComponent / Quote | Pricing |
| Agency commercial relationship (marketplace) | AgencyMarketplace |
| AgencyProfile (marketplace display / contact / commercial posture) | AgencyMarketplace |
| Agency identity (`PartyKind.Agency`) | Party |
| VisaType / visa requirements | Visa |
| Booking / accepted quote snapshot | Booking |
| PaymentAttempt / refund foundation | Payment |
| Provider hotel mapping / live room rate | HotelBooking |
| Live flight offer / provider booking | Flight |
| SearchDocument / facets | Search |
| SeoRoute / canonical / redirect | SEO |
| Email/SMS delivery | Notification |
| Admin UI forms | Presentation (not a domain module) |
