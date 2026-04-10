"use client";

import Link from "next/link";
import { Star, Clock, ArrowRight } from "lucide-react";

interface Equipment {
  id: string;
  name: string;
  category: string;
  image: string;
  hourlyRate: number;
  dailyRate: number;
  weeklyRate: number;
  rating: number;
  reviewCount: number;
  available: boolean;
}

const FEATURED_EQUIPMENT: Equipment[] = [
  {
    id: "1",
    name: "DeWalt 20V MAX Drill Kit",
    category: "Construction",
    image:
      "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=400&q=80",
    hourlyRate: 8,
    dailyRate: 35,
    weeklyRate: 150,
    rating: 4.9,
    reviewCount: 128,
    available: true,
  },
  {
    id: "2",
    name: "Honda 2200W Generator",
    category: "Electrical",
    image:
      "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=400&q=80",
    hourlyRate: 15,
    dailyRate: 65,
    weeklyRate: 280,
    rating: 4.8,
    reviewCount: 95,
    available: true,
  },
  {
    id: "3",
    name: "Stihl Professional Chainsaw",
    category: "Landscaping",
    image:
      "https://images.unsplash.com/photo-1558904541-efa843a96f01?w=400&q=80",
    hourlyRate: 12,
    dailyRate: 50,
    weeklyRate: 200,
    rating: 4.7,
    reviewCount: 76,
    available: false,
  },
  {
    id: "4",
    name: "Concrete Mixer 120L",
    category: "Construction",
    image:
      "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=400&q=80",
    hourlyRate: 20,
    dailyRate: 85,
    weeklyRate: 350,
    rating: 4.6,
    reviewCount: 54,
    available: true,
  },
  {
    id: "5",
    name: "Kärcher Pressure Washer",
    category: "Cleaning",
    image:
      "https://images.unsplash.com/photo-1558317374-067fb5f30001?w=400&q=80",
    hourlyRate: 10,
    dailyRate: 45,
    weeklyRate: 180,
    rating: 4.8,
    reviewCount: 112,
    available: true,
  },
  {
    id: "6",
    name: "Wagner Paint Sprayer Pro",
    category: "Decorating",
    image:
      "https://images.unsplash.com/photo-1562259949-e8e7689d7828?w=400&q=80",
    hourlyRate: 9,
    dailyRate: 40,
    weeklyRate: 160,
    rating: 4.5,
    reviewCount: 68,
    available: true,
  },
];

export function FeaturedEquipment() {
  return (
    <section className="py-24 bg-[#F8F8F8]">
      <div className="max-w-7xl mx-auto px-4">
        {/* Section Header */}
        <div className="flex flex-col md:flex-row md:items-end justify-between mb-14">
          <div>
            <h2 className="text-4xl md:text-5xl font-extrabold text-[#111111] mb-4">
              Ready to Rent Today
            </h2>
            <p className="text-[#666666] text-xl max-w-xl leading-relaxed">
              Our most requested equipment, available now with
              same-day delivery.
            </p>
          </div>
          <Link
            href="/equipment"
            className="inline-flex items-center gap-2 text-accent font-bold text-lg hover:gap-4 transition-all mt-6 md:mt-0 group"
          >
            View All Equipment
            <ArrowRight className="w-6 h-6 group-hover:translate-x-1 transition-transform" />
          </Link>
        </div>

        {/* Equipment Grid */}
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-8">
          {FEATURED_EQUIPMENT.map((item) => (
            <Link
              key={item.id}
              href={`/equipment/${item.id}`}
              className="group bg-white border-2 border-gray-200 rounded-3xl overflow-hidden hover:border-accent transition-all duration-300 hover:shadow-2xl hover:scale-[1.02]"
            >
              {/* Image */}
              <div className="relative h-48 overflow-hidden">
                <img
                  src={item.image}
                  alt={item.name}
                  className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                />
                <div className="absolute top-4 left-4">
                  <span className="bg-accent text-black text-xs font-semibold px-3 py-1 rounded-full">
                    {item.category}
                  </span>
                </div>
                <div className="absolute top-4 right-4">
                  <span
                    className={`text-xs font-semibold px-3 py-1 rounded-full ${
                      item.available
                        ? "bg-green-500/90 text-white"
                        : "bg-red-500/90 text-white"
                    }`}
                  >
                    {item.available ? "Available" : "Rented"}
                  </span>
                </div>
              </div>

              {/* Content */}
              <div className="p-6">
                <div className="flex items-center gap-2 mb-3">
                  <div className="flex items-center gap-1">
                    <Star className="w-5 h-5 text-accent fill-accent" />
                    <span className="text-[#111111] font-bold text-base">
                      {item.rating}
                    </span>
                  </div>
                  <span className="text-[#999] text-sm">
                    ({item.reviewCount} reviews)
                  </span>
                </div>

                <h3 className="text-xl font-bold text-[#111111] mb-4 group-hover:text-accent transition-colors duration-300 line-clamp-2 min-h-[3.5rem]">
                  {item.name}
                </h3>

                {/* Pricing */}
                <div className="bg-[#F8F8F8] rounded-2xl p-4 mb-5">
                  <div className="flex items-baseline gap-2 mb-2">
                    <span className="text-3xl font-extrabold text-accent">
                      ${item.dailyRate}
                    </span>
                    <span className="text-[#999] text-sm font-medium">/day</span>
                  </div>
                  <div className="flex items-center gap-4 text-sm text-[#666666]">
                    <div className="flex items-center gap-1">
                      <Clock className="w-4 h-4" />
                      <span>${item.hourlyRate}/hr</span>
                    </div>
                    <span className="text-[#999]">•</span>
                    <span>${item.weeklyRate}/wk</span>
                  </div>
                </div>

                <button className="w-full bg-accent hover:bg-[#C97F00] text-black font-bold py-4 rounded-xl transition-all duration-200 shadow-md hover:shadow-lg">
                  Rent Now
                </button>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}
