"use client";

import Link from "next/link";
import { ArrowRight } from "lucide-react";

const CATEGORIES = [
  {
    name: "Construction Equipment",
    description: "Excavators, concrete mixers, scaffolding, power tools",
    count: 85,
    href: "/equipment?category=construction",
    image:
      "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=800&q=80",
  },
  {
    name: "Landscaping Tools",
    description: "Lawn mowers, chainsaws, hedge trimmers, leaf blowers",
    count: 62,
    href: "/equipment?category=landscaping",
    image:
      "https://images.unsplash.com/photo-1558904541-efa843a96f01?w=800&q=80",
  },
  {
    name: "Plumbing Equipment",
    description: "Pipe cutters, drain cleaners, welding machines",
    count: 48,
    href: "/equipment?category=plumbing",
    image:
      "https://images.unsplash.com/photo-1585704032915-c3400ca199e7?w=800&q=80",
  },
  {
    name: "Electrical & Heating",
    description: "Generators, heat guns, cable tools, testing equipment",
    count: 55,
    href: "/equipment?category=electrical",
    image:
      "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=800&q=80",
  },
  {
    name: "Decorating Tools",
    description: "Paint sprayers, sanders, wallpaper steamers",
    count: 42,
    href: "/equipment?category=decorating",
    image:
      "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=800&q=80",
  },
  {
    name: "Cleaning Equipment",
    description: "Pressure washers, floor cleaners, industrial vacuums",
    count: 38,
    href: "/equipment?category=cleaning",
    image:
      "https://images.unsplash.com/photo-1558317374-067fb5f30001?w=800&q=80",
  },
];

export function Categories() {
  return (
    <section className="py-24 bg-[#F5F5F5]">
      <div className="max-w-7xl mx-auto px-4">
        {/* Section Header */}
        <div className="text-center mb-16">
          
          <h2 className="text-4xl md:text-5xl font-extrabold text-[#111111] mb-6">
            Browse Equipment by Category
          </h2>
          <p className="text-[#666666] text-xl max-w-3xl mx-auto leading-relaxed">
            From construction machinery to precision tools, find everything you
            need for your project in our comprehensive catalogue.
          </p>
        </div>

        {/* Categories Grid */}
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-8">
          {CATEGORIES.map((category) => (
            <Link
              key={category.name}
              href={category.href}
              className="group relative h-[420px] rounded-3xl overflow-hidden hover:shadow-2xl transition-all duration-500 hover:scale-[1.02]"
            >
              {/* Background Image */}
              <div className="absolute inset-0">
                <img
                  src={category.image}
                  alt={category.name}
                  className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                />
              </div>

              {/* Gradient Overlay - lighter on hover to show more image */}
              <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/40 to-black/20 group-hover:from-black/60 group-hover:via-black/20 group-hover:to-transparent transition-all duration-500"></div>

              {/* Content */}
              <div className="relative h-full flex flex-col justify-end p-8">
                <div className="flex items-center justify-between mb-4">
                  <span className="bg-accent text-black text-sm font-bold px-4 py-2 rounded-full">
                    {category.count} items
                  </span>
                </div>

                <h3 className="text-3xl font-bold text-white mb-3 group-hover:text-accent transition-colors duration-300">
                  {category.name}
                </h3>
                <p className="text-white/90 text-base mb-6 leading-relaxed">
                  {category.description}
                </p>

                <div className="flex items-center gap-2 text-accent font-bold text-lg">
                  View Equipment
                  <ArrowRight className="w-5 h-5 group-hover:translate-x-2 transition-transform duration-300" />
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}

