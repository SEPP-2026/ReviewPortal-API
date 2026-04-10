import Link from "next/link";
import { Check, ArrowRight, Calculator } from "lucide-react";

const PRICING_PLANS = [
  {
    name: "Hourly",
    description: "Perfect for quick jobs",
    highlight: false,
    features: [
      "Minimum 2-hour rental",
      "Best for small repairs",
      "No deposit required",
      "Basic support included",
    ],
    examples: [
      { name: "Power Drill", price: "$8/hr" },
      { name: "Pressure Washer", price: "$10/hr" },
      { name: "Generator", price: "$15/hr" },
    ],
  },
  {
    name: "Daily",
    description: "Most popular choice",
    highlight: true,
    badge: "BEST VALUE",
    features: [
      "24-hour rental period",
      "Up to 40% savings vs hourly",
      "Free damage waiver",
      "Priority support",
      "Flexible pickup times",
    ],
    examples: [
      { name: "Power Drill", price: "$35/day" },
      { name: "Pressure Washer", price: "$45/day" },
      { name: "Generator", price: "$65/day" },
    ],
  },
  {
    name: "Weekly",
    description: "For ongoing projects",
    highlight: false,
    features: [
      "7-day rental period",
      "Up to 60% savings vs daily",
      "Free delivery over $200",
      "Dedicated support line",
      "Equipment swap available",
    ],
    examples: [
      { name: "Power Drill", price: "$150/wk" },
      { name: "Pressure Washer", price: "$180/wk" },
      { name: "Generator", price: "$280/wk" },
    ],
  },
];

const INCLUDED_FEATURES = [
  "All equipment fully maintained",
  "Safety instructions provided",
  "Accessories included",
  "24/7 technical support",
  "Insurance coverage",
  "Flexible returns",
];

export const metadata = {
  title: "Pricing Plans | Shelton Tool-Hire",
  description:
    "Transparent pricing for equipment rental. Hourly, daily, and weekly rates available.",
};

export default function PricingPage() {
  return (
    <div className="min-h-screen bg-[#F2F2F2] pt-40 pb-20">
      <div className="max-w-7xl mx-auto px-4">
        {/* Header */}
        <div className="text-center mb-16">
          <span className="inline-block text-accent font-semibold text-sm uppercase tracking-wider mb-4">
            Simple Pricing
          </span>
          <h1 className="text-4xl md:text-5xl font-bold text-[#111111] mb-4">
            Flexible Rental Plans
          </h1>
          <p className="text-[#666666] text-lg max-w-2xl mx-auto">
            Choose the rental period that suits your project. The longer you
            rent, the more you save.
          </p>
        </div>

        {/* Pricing Cards */}
        <div className="grid md:grid-cols-3 gap-6 mb-16">
          {PRICING_PLANS.map((plan) => (
            <div
              key={plan.name}
              className={`relative rounded-2xl p-6 ${
                plan.highlight
                  ? "bg-accent text-black ring-4 ring-accent/30"
                  : "bg-white border border-gray-200 text-[#111111]"
              }`}
            >
              {plan.badge && (
                <span className="absolute -top-3 left-1/2 -translate-x-1/2 bg-[#111111] text-white text-xs font-bold px-4 py-1 rounded-full">
                  {plan.badge}
                </span>
              )}

              <div className="text-center mb-6">
                <h3 className="text-2xl font-bold mb-2">{plan.name}</h3>
                <p className={plan.highlight ? "text-black/70" : "text-[#666666]"}>
                  {plan.description}
                </p>
              </div>

              {/* Features */}
              <ul className="space-y-3 mb-6">
                {plan.features.map((feature) => (
                  <li key={feature} className="flex items-center gap-2">
                    <Check
                      className={`w-5 h-5 ${
                        plan.highlight ? "text-black" : "text-accent"
                      }`}
                    />
                    <span
                      className={plan.highlight ? "text-black/80" : "text-[#666666]"}
                    >
                      {feature}
                    </span>
                  </li>
                ))}
              </ul>

              {/* Example Prices */}
              <div
                className={`rounded-xl p-4 mb-6 ${
                  plan.highlight ? "bg-black/10" : "bg-[#F2F2F2]"
                }`}
              >
                <p
                  className={`text-sm font-medium mb-3 ${
                    plan.highlight ? "text-black/70" : "text-[#666666]"
                  }`}
                >
                  Example Prices:
                </p>
                {plan.examples.map((example) => (
                  <div
                    key={example.name}
                    className="flex justify-between items-center py-1"
                  >
                    <span
                      className={plan.highlight ? "text-black/80" : "text-[#666666]"}
                    >
                      {example.name}
                    </span>
                    <span
                      className={`font-bold ${
                        plan.highlight ? "text-black" : "text-accent"
                      }`}
                    >
                      {example.price}
                    </span>
                  </div>
                ))}
              </div>

              <Link
                href="/equipment"
                className={`block w-full text-center py-3 rounded-lg font-semibold transition-colors ${
                  plan.highlight
                    ? "bg-black text-white hover:bg-black/90"
                    : "bg-accent/10 text-accent hover:bg-accent hover:text-black"
                }`}
              >
                Browse Equipment
              </Link>
            </div>
          ))}
        </div>

        {/* Calculator CTA */}
        <div className="bg-white border border-gray-200 rounded-2xl p-8 mb-16 text-center">
          <Calculator className="w-12 h-12 text-accent mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-[#111111] mb-4">
            Need an Exact Quote?
          </h2>
          <p className="text-[#666666] mb-6 max-w-lg mx-auto">
            Use our rental calculator to get accurate pricing for your specific
            equipment and rental duration.
          </p>
          <Link
            href="/equipment"
            className="inline-flex items-center gap-2 bg-accent hover:bg-[#C97F00] text-black font-semibold px-8 py-4 rounded-lg transition-colors"
          >
            Calculate Rental Cost
            <ArrowRight className="w-5 h-5" />
          </Link>
        </div>

        {/* Included Features */}
        <div className="text-center">
          <h2 className="text-2xl font-bold text-[#111111] mb-8">
            All Rentals Include
          </h2>
          <div className="grid sm:grid-cols-2 md:grid-cols-3 gap-4">
            {INCLUDED_FEATURES.map((feature) => (
              <div
                key={feature}
                className="flex items-center justify-center gap-3 bg-white border border-gray-200 rounded-xl p-4"
              >
                <Check className="w-5 h-5 text-accent" />
                <span className="text-[#111111]">{feature}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
