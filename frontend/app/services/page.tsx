import Link from "next/link";
import {
  Truck,
  Wrench,
  Clock,
  Shield,
  Phone,
  Users,
  Settings,
  CheckCircle,
  ArrowRight,
} from "lucide-react";

const SERVICES = [
  {
    icon: Wrench,
    title: "Equipment Rental",
    description:
      "Wide range of professional tools and equipment available for hourly, daily, or weekly rental. From power tools to heavy machinery.",
    features: [
      "500+ tools available",
      "Flexible rental periods",
      "Competitive pricing",
      "Quality guaranteed",
    ],
  },
  {
    icon: Truck,
    title: "Delivery & Collection",
    description:
      "We deliver equipment directly to your job site and collect it when you're done. Free delivery on orders over $200.",
    features: [
      "Same-day delivery",
      "Free over $200",
      "Scheduled pickups",
      "GPS tracking",
    ],
  },
  {
    icon: Settings,
    title: "Equipment Training",
    description:
      "Get proper training on how to use equipment safely and efficiently. Our experts will show you the ropes.",
    features: [
      "Safety briefings",
      "Usage tutorials",
      "Best practices",
      "Certificate provided",
    ],
  },
  {
    icon: Shield,
    title: "Insurance Coverage",
    description:
      "All our equipment comes with comprehensive insurance coverage. Rent with peace of mind.",
    features: [
      "Damage protection",
      "Theft coverage",
      "Liability included",
      "Easy claims",
    ],
  },
  {
    icon: Clock,
    title: "24/7 Support",
    description:
      "Round-the-clock technical support for any issues you might face. We're always here to help.",
    features: [
      "Phone support",
      "Email assistance",
      "On-site help",
      "Emergency repairs",
    ],
  },
  {
    icon: Users,
    title: "Corporate Accounts",
    description:
      "Special pricing and benefits for businesses with regular rental needs. Build a lasting partnership.",
    features: [
      "Volume discounts",
      "Priority booking",
      "Dedicated manager",
      "Monthly billing",
    ],
  },
];

export const metadata = {
  title: "Our Services | Shelton Tool-Hire",
  description:
    "Explore our comprehensive equipment rental services including delivery, training, and 24/7 support.",
};

export default function ServicesPage() {
  return (
    <div className="min-h-screen bg-[#F2F2F2] pt-40 pb-20">
      <div className="max-w-7xl mx-auto px-4">
        {/* Header */}
        <div className="text-center mb-16">
          <span className="inline-block text-accent font-semibold text-sm uppercase tracking-wider mb-4">
            What We Offer
          </span>
          <h1 className="text-4xl md:text-5xl font-bold text-[#111111] mb-4">
            Our Services
          </h1>
          <p className="text-[#666666] text-lg max-w-2xl mx-auto">
            From equipment rental to delivery and technical support, we provide
            comprehensive services to make your project a success.
          </p>
        </div>

        {/* Services Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 mb-20">
          {SERVICES.map((service) => (
            <div
              key={service.title}
              className="bg-white border border-gray-200 rounded-2xl p-6 hover:border-accent/30 hover:shadow-lg transition-all duration-300"
            >
              <div className="w-14 h-14 bg-accent/20 rounded-xl flex items-center justify-center mb-6">
                <service.icon className="w-7 h-7 text-accent" />
              </div>
              <h3 className="text-xl font-bold text-[#111111] mb-3">
                {service.title}
              </h3>
              <p className="text-[#666666] mb-6">{service.description}</p>
              <ul className="space-y-2">
                {service.features.map((feature) => (
                  <li key={feature} className="flex items-center gap-2">
                    <CheckCircle className="w-4 h-4 text-accent" />
                    <span className="text-[#666666] text-sm">{feature}</span>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* CTA Section */}
        <div className="bg-[#111111] rounded-3xl p-8 md:p-12 text-center">
          <h2 className="text-3xl font-bold text-white mb-4">
            Ready to Get Started?
          </h2>
          <p className="text-white/70 text-lg mb-8 max-w-xl mx-auto">
            Browse our equipment catalogue or contact us to discuss your
            project requirements.
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            <Link
              href="/equipment"
              className="inline-flex items-center gap-2 bg-accent hover:bg-[#C97F00] text-black font-semibold px-8 py-4 rounded-lg transition-colors"
            >
              Browse Equipment
              <ArrowRight className="w-5 h-5" />
            </Link>
            <Link
              href="/contact"
              className="inline-flex items-center gap-2 border-2 border-white/30 hover:border-white text-white font-semibold px-8 py-4 rounded-lg transition-colors"
            >
              <Phone className="w-5 h-5" />
              Contact Us
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
