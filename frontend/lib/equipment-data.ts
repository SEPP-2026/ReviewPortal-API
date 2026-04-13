export interface Equipment {
  id: string;
  name: string;
  slug: string;
  category: string;
  categorySlug: string;
  description: string;
  image: string;
  images: string[];
  hourlyRate: number;
  dailyRate: number;
  weeklyRate: number;
  rating: number;
  reviewCount: number;
  available: boolean;
  specifications: Record<string, string>;
  features: string[];
}

export const EQUIPMENT_DATA: Equipment[] = [
  // Construction Equipment
  {
    id: "1",
    name: "DeWalt 20V MAX Drill Kit",
    slug: "dewalt-20v-max-drill-kit",
    category: "Construction",
    categorySlug: "construction",
    description:
      "Professional-grade cordless drill with two batteries, charger, and carrying case. Perfect for drilling and driving in wood, metal, and plastic.",
    image:
      "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&q=80",
    ],
    hourlyRate: 8,
    dailyRate: 35,
    weeklyRate: 150,
    rating: 4.9,
    reviewCount: 128,
    available: true,
    specifications: {
      "Voltage": "20V MAX",
      "Speed": "0-550 / 0-2000 RPM",
      "Torque": "340 UWO",
      "Chuck Size": "1/2 inch",
      "Weight": "3.5 lbs",
    },
    features: [
      "Brushless motor for efficiency",
      "LED work light",
      "Belt clip included",
      "Two batteries included",
    ],
  },
  {
    id: "2",
    name: "Concrete Mixer 120L",
    slug: "concrete-mixer-120l",
    category: "Construction",
    categorySlug: "construction",
    description:
      "Heavy-duty electric concrete mixer with 120L drum capacity. Ideal for small to medium construction projects.",
    image:
      "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=600&q=80",
    ],
    hourlyRate: 20,
    dailyRate: 85,
    weeklyRate: 350,
    rating: 4.6,
    reviewCount: 54,
    available: true,
    specifications: {
      "Drum Capacity": "120 Liters",
      "Motor": "550W Electric",
      "Rotation Speed": "28 RPM",
      "Weight": "45 kg",
      "Power Supply": "230V / 50Hz",
    },
    features: [
      "Portable wheeled design",
      "Easy drum cleaning",
      "Safety guard included",
      "Adjustable mixing angle",
    ],
  },
  {
    id: "3",
    name: "Scaffolding Tower System",
    slug: "scaffolding-tower-system",
    category: "Construction",
    categorySlug: "construction",
    description:
      "Aluminum scaffolding tower with adjustable height up to 6 meters. Includes safety rails and locking casters.",
    image:
      "https://images.unsplash.com/photo-1541888946425-d81bb19240f5?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1541888946425-d81bb19240f5?w=600&q=80",
    ],
    hourlyRate: 25,
    dailyRate: 100,
    weeklyRate: 400,
    rating: 4.7,
    reviewCount: 42,
    available: true,
    specifications: {
      "Max Height": "6 meters",
      "Platform Size": "1.2m x 2m",
      "Load Capacity": "200 kg",
      "Material": "Aluminum",
      "Caster Size": "150mm",
    },
    features: [
      "Quick assembly system",
      "Safety guardrails",
      "Locking casters",
      "Transport wheels included",
    ],
  },

  // Landscaping Tools
  {
    id: "4",
    name: "Stihl Professional Chainsaw",
    slug: "stihl-professional-chainsaw",
    category: "Landscaping",
    categorySlug: "landscaping",
    description:
      "Powerful petrol chainsaw for professional tree cutting and logging. Features anti-vibration technology and quick chain tensioning.",
    image:
      "https://images.unsplash.com/photo-1558904541-efa843a96f01?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1558904541-efa843a96f01?w=600&q=80",
    ],
    hourlyRate: 12,
    dailyRate: 50,
    weeklyRate: 200,
    rating: 4.7,
    reviewCount: 76,
    available: false,
    specifications: {
      "Engine": "50cc 2-stroke",
      "Bar Length": "18 inches",
      "Chain Speed": "21 m/s",
      "Weight": "5.2 kg",
      "Tank Capacity": "0.47L",
    },
    features: [
      "Anti-vibration system",
      "Quick chain tensioning",
      "Automatic chain oiler",
      "Safety chain brake",
    ],
  },
  {
    id: "5",
    name: "Honda Ride-On Mower",
    slug: "honda-ride-on-mower",
    category: "Landscaping",
    categorySlug: "landscaping",
    description:
      "Commercial-grade ride-on mower with 42-inch cutting deck. Perfect for large lawns and professional landscaping.",
    image:
      "https://images.unsplash.com/photo-1592417817098-8fd3d9eb14a5?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1592417817098-8fd3d9eb14a5?w=600&q=80",
    ],
    hourlyRate: 35,
    dailyRate: 150,
    weeklyRate: 600,
    rating: 4.8,
    reviewCount: 38,
    available: true,
    specifications: {
      "Engine": "22HP V-Twin",
      "Cutting Width": "42 inches",
      "Speed": "0-8 mph",
      "Fuel Tank": "3.3 gallons",
      "Turning Radius": "16 inches",
    },
    features: [
      "Hydrostatic transmission",
      "Mulching capability",
      "Cruise control",
      "LED headlights",
    ],
  },
  {
    id: "6",
    name: "Electric Hedge Trimmer",
    slug: "electric-hedge-trimmer",
    category: "Landscaping",
    categorySlug: "landscaping",
    description:
      "Lightweight electric hedge trimmer with laser-cut blades. Ideal for shaping hedges and shrubs.",
    image:
      "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=600&q=80",
    ],
    hourlyRate: 6,
    dailyRate: 25,
    weeklyRate: 100,
    rating: 4.5,
    reviewCount: 92,
    available: true,
    specifications: {
      "Motor": "500W Electric",
      "Blade Length": "24 inches",
      "Cutting Capacity": "22mm",
      "Weight": "3.2 kg",
      "Cable Length": "10m",
    },
    features: [
      "Laser-cut blades",
      "Ergonomic handle",
      "Safety switch",
      "Low noise operation",
    ],
  },

  // Plumbing Equipment
  {
    id: "7",
    name: "Pipe Cutter Set Pro",
    slug: "pipe-cutter-set-pro",
    category: "Plumbing",
    categorySlug: "plumbing",
    description:
      "Complete professional pipe cutter set for copper, steel, and PVC pipes. Includes multiple cutter sizes and deburring tools.",
    image:
      "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=600&q=80",
    ],
    hourlyRate: 5,
    dailyRate: 20,
    weeklyRate: 80,
    rating: 4.6,
    reviewCount: 65,
    available: true,
    specifications: {
      "Pipe Sizes": "1/8\" to 2\"",
      "Materials": "Copper, Steel, PVC",
      "Pieces": "12-piece set",
      "Case": "Heavy-duty carry case",
      "Spare Wheels": "4 included",
    },
    features: [
      "Hardened steel wheels",
      "Quick-adjust mechanism",
      "Deburring tools included",
      "Ergonomic handles",
    ],
  },
  {
    id: "8",
    name: "Electric Drain Cleaner",
    slug: "electric-drain-cleaner",
    category: "Plumbing",
    categorySlug: "plumbing",
    description:
      "Powerful electric drain snake for clearing blockages in pipes up to 4 inches. Features variable speed control and foot pedal operation.",
    image:
      "https://images.unsplash.com/photo-1607472586893-edb57bdc0e39?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1607472586893-edb57bdc0e39?w=600&q=80",
    ],
    hourlyRate: 15,
    dailyRate: 60,
    weeklyRate: 240,
    rating: 4.4,
    reviewCount: 48,
    available: true,
    specifications: {
      "Motor": "750W",
      "Cable Length": "75 ft",
      "Pipe Range": "1.5\" - 4\"",
      "RPM": "0-400",
      "Weight": "45 lbs",
    },
    features: [
      "Variable speed control",
      "Foot pedal operation",
      "Auto-feed function",
      "Multiple attachments",
    ],
  },

  // Electrical Tools
  {
    id: "9",
    name: "Honda 2200W Generator",
    slug: "honda-2200w-generator",
    category: "Electrical",
    categorySlug: "electrical",
    description:
      "Quiet, portable inverter generator perfect for job sites and outdoor events. Super quiet operation at 48-57 dBA.",
    image:
      "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=600&q=80",
    ],
    hourlyRate: 15,
    dailyRate: 65,
    weeklyRate: 280,
    rating: 4.8,
    reviewCount: 95,
    available: true,
    specifications: {
      "Output": "2200W peak / 1800W rated",
      "Engine": "121cc Honda GXR120",
      "Run Time": "8.1 hrs @ 25% load",
      "Noise Level": "48-57 dBA",
      "Weight": "47 lbs",
    },
    features: [
      "Inverter technology",
      "Parallel capable",
      "Eco-Throttle system",
      "Oil alert system",
    ],
  },
  {
    id: "10",
    name: "Industrial Heat Gun Kit",
    slug: "industrial-heat-gun-kit",
    category: "Electrical",
    categorySlug: "electrical",
    description:
      "Variable temperature heat gun with LCD display and multiple nozzle attachments. Ideal for paint stripping and shrink wrapping.",
    image:
      "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=600&q=80",
    ],
    hourlyRate: 4,
    dailyRate: 18,
    weeklyRate: 70,
    rating: 4.5,
    reviewCount: 72,
    available: true,
    specifications: {
      "Power": "2000W",
      "Temperature": "50-650°C",
      "Airflow": "500L/min",
      "Display": "LCD Digital",
      "Nozzles": "5 included",
    },
    features: [
      "Variable temperature",
      "LCD display",
      "Overheat protection",
      "Stand mode",
    ],
  },

  // Decorating Tools
  {
    id: "11",
    name: "Wagner Paint Sprayer Pro",
    slug: "wagner-paint-sprayer-pro",
    category: "Decorating",
    categorySlug: "decorating",
    description:
      "Professional HVLP paint sprayer for interior and exterior painting. Adjustable spray patterns and flow control.",
    image:
      "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=600&q=80",
    ],
    hourlyRate: 9,
    dailyRate: 40,
    weeklyRate: 160,
    rating: 4.5,
    reviewCount: 68,
    available: true,
    specifications: {
      "Power": "600W",
      "Tank Capacity": "1.5L",
      "Spray Width": "1-25cm",
      "Flow Rate": "0-500ml/min",
      "Hose Length": "6m",
    },
    features: [
      "HVLP technology",
      "Adjustable spray patterns",
      "Easy clean system",
      "Shoulder strap included",
    ],
  },
  {
    id: "12",
    name: "Random Orbital Sander",
    slug: "random-orbital-sander",
    category: "Decorating",
    categorySlug: "decorating",
    description:
      "Professional random orbital sander with variable speed and dust collection. Perfect for preparing surfaces before painting.",
    image:
      "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&q=80",
    ],
    hourlyRate: 5,
    dailyRate: 22,
    weeklyRate: 90,
    rating: 4.7,
    reviewCount: 85,
    available: true,
    specifications: {
      "Power": "400W",
      "Pad Size": "125mm",
      "Orbits": "12000 OPM",
      "Dust Collection": "Built-in bag",
      "Weight": "1.3 kg",
    },
    features: [
      "Variable speed dial",
      "Hook & loop pad",
      "Dust sealed switch",
      "Ergonomic grip",
    ],
  },

  // Cleaning Equipment
  {
    id: "13",
    name: "Kärcher Pressure Washer Pro",
    slug: "karcher-pressure-washer-pro",
    category: "Cleaning",
    categorySlug: "cleaning",
    description:
      "High-pressure washer with 150 bar pressure. Ideal for cleaning driveways, patios, vehicles, and building exteriors.",
    image:
      "https://images.unsplash.com/photo-1558317374-067fb5f30001?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1558317374-067fb5f30001?w=600&q=80",
    ],
    hourlyRate: 10,
    dailyRate: 45,
    weeklyRate: 180,
    rating: 4.8,
    reviewCount: 112,
    available: true,
    specifications: {
      "Pressure": "150 bar max",
      "Flow Rate": "500 L/hr",
      "Motor": "2100W",
      "Hose Length": "8m",
      "Weight": "18 kg",
    },
    features: [
      "Multiple spray patterns",
      "Detergent tank",
      "Quick-connect fittings",
      "Transport wheels",
    ],
  },
  {
    id: "14",
    name: "Industrial Wet/Dry Vacuum",
    slug: "industrial-wet-dry-vacuum",
    category: "Cleaning",
    categorySlug: "cleaning",
    description:
      "Heavy-duty wet and dry vacuum cleaner with 70L capacity. Perfect for construction site cleanup and industrial applications.",
    image:
      "https://images.unsplash.com/photo-1527515637462-cff94eecc1ac?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1527515637462-cff94eecc1ac?w=600&q=80",
    ],
    hourlyRate: 8,
    dailyRate: 35,
    weeklyRate: 140,
    rating: 4.6,
    reviewCount: 58,
    available: true,
    specifications: {
      "Capacity": "70 Liters",
      "Motor": "2400W twin motor",
      "Suction": "240 mbar",
      "Hose Length": "5m",
      "Power Outlet": "Yes",
    },
    features: [
      "Wet and dry pickup",
      "Auto filter cleaning",
      "Tool power outlet",
      "Stainless steel tank",
    ],
  },
  {
    id: "15",
    name: "Floor Scrubber Machine",
    slug: "floor-scrubber-machine",
    category: "Cleaning",
    categorySlug: "cleaning",
    description:
      "Walk-behind floor scrubber for commercial cleaning. Scrubs and dries floors in a single pass.",
    image:
      "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=600&q=80",
    ],
    hourlyRate: 18,
    dailyRate: 75,
    weeklyRate: 300,
    rating: 4.7,
    reviewCount: 34,
    available: true,
    specifications: {
      "Cleaning Width": "17 inches",
      "Tank Capacity": "9 gallons each",
      "Runtime": "2.5 hours",
      "Brush Speed": "175 RPM",
      "Weight": "145 lbs",
    },
    features: [
      "Scrub and dry in one pass",
      "Squeegee system",
      "Quick-change pads",
      "Quiet operation",
    ],
  },
];

export const CATEGORIES = [
  { name: "All Equipment", slug: "all" },
  { name: "Construction", slug: "construction" },
  { name: "Landscaping", slug: "landscaping" },
  { name: "Plumbing", slug: "plumbing" },
  { name: "Electrical", slug: "electrical" },
  { name: "Decorating", slug: "decorating" },
  { name: "Cleaning", slug: "cleaning" },
];
