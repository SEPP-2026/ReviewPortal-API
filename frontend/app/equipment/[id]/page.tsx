"use client";

import { useState } from "react";
import Link from "next/link";
import { notFound } from "next/navigation";
import {
  Star,
  Clock,
  ArrowLeft,
  Check,
  Calendar,
  Truck,
  Shield,
  Phone,
  Calculator,
} from "lucide-react";
import { EQUIPMENT_DATA } from "@/lib/equipment-data";

interface EquipmentDetailProps {
  params: { id: string };
}

export default function EquipmentDetailPage({ params }: EquipmentDetailProps) {
  const equipment = EQUIPMENT_DATA.find((item) => item.id === params.id);
  const [rentalPeriod, setRentalPeriod] = useState<"hourly" | "daily" | "weekly">("daily");
  const [quantity, setQuantity] = useState(1);
  const [duration, setDuration] = useState(1);

  if (!equipment) {
    notFound();
  }

  const calculateTotal = () => {
    const rate =
      rentalPeriod === "hourly"
        ? equipment.hourlyRate
        : rentalPeriod === "daily"
        ? equipment.dailyRate
        : equipment.weeklyRate;
    return rate * quantity * duration;
  };

  const relatedEquipment = EQUIPMENT_DATA.filter(
    (item) =>
      item.categorySlug === equipment.categorySlug && item.id !== equipment.id
  ).slice(0, 4);

  return (
    <div className="min-h-screen bg-[#F2F2F2] pt-40 pb-20">
      <div className="max-w-7xl mx-auto px-4">
        {/* Breadcrumb */}
        <nav className="flex items-center gap-2 text-sm mb-8">
          <Link href="/" className="text-[#666666] hover:text-accent transition-colors">
            Home
          </Link>
          <span className="text-[#666666]">/</span>
          <Link href="/equipment" className="text-[#666666] hover:text-accent transition-colors">
            Equipment
          </Link>
          <span className="text-[#666666]">/</span>
          <Link
            href={`/equipment?category=${equipment.categorySlug}`}
            className="text-[#666666] hover:text-accent transition-colors"
          >
            {equipment.category}
          </Link>
          <span className="text-[#666666]">/</span>
          <span className="text-[#111111]">{equipment.name}</span>
        </nav>

        {/* Back Button */}
        <Link
          href="/equipment"
          className="inline-flex items-center gap-2 text-[#666666] hover:text-accent transition-colors mb-8"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Equipment
        </Link>

        {/* Main Content */}
        <div className="grid lg:grid-cols-2 gap-12">
          {/* Image Gallery */}
          <div>
            <div className="relative bg-white border border-gray-200 rounded-2xl overflow-hidden mb-4">
              <img
                src={equipment.image}
                alt={equipment.name}
                className="w-full aspect-square object-cover"
              />
              <div className="absolute top-4 left-4">
                <span className="bg-accent text-black text-sm font-semibold px-3 py-1 rounded-full">
                  {equipment.category}
                </span>
              </div>
              <div className="absolute top-4 right-4">
                <span
                  className={`text-sm font-semibold px-3 py-1 rounded-full ${
                    equipment.available
                      ? "bg-green-500 text-white"
                      : "bg-red-500 text-white"
                  }`}
                >
                  {equipment.available ? "Available" : "Currently Rented"}
                </span>
              </div>
            </div>
          </div>

          {/* Details */}
          <div>
            {/* Rating */}
            <div className="flex items-center gap-3 mb-4">
              <div className="flex items-center gap-1">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className={`w-5 h-5 ${
                      i < Math.floor(equipment.rating)
                        ? "text-accent fill-accent"
                        : "text-gray"
                    }`}
                  />
                ))}
              </div>
              <span className="text-[#111111] font-semibold">{equipment.rating}</span>
              <span className="text-[#666666]">({equipment.reviewCount} reviews)</span>
            </div>

            {/* Title */}
            <h1 className="text-3xl md:text-4xl font-bold text-[#111111] mb-4">
              {equipment.name}
            </h1>

            {/* Description */}
            <p className="text-[#666666] text-lg mb-8 leading-relaxed">
              {equipment.description}
            </p>

            {/* Pricing Cards */}
            <div className="grid grid-cols-3 gap-4 mb-8">
              <button
                onClick={() => setRentalPeriod("hourly")}
                className={`p-4 rounded-xl text-center transition-all ${
                  rentalPeriod === "hourly"
                    ? "bg-accent text-black ring-2 ring-accent"
                    : "bg-white border border-gray-200 text-[#111111] hover:border-accent/50"
                }`}
              >
                <div className="flex items-center justify-center gap-1 mb-2">
                  <Clock className="w-4 h-4" />
                  <span className="text-sm font-medium">Hourly</span>
                </div>
                <span className="text-2xl font-bold">${equipment.hourlyRate}</span>
                <span className="text-sm opacity-70">/hr</span>
              </button>
              <button
                onClick={() => setRentalPeriod("daily")}
                className={`p-4 rounded-xl text-center transition-all ${
                  rentalPeriod === "daily"
                    ? "bg-accent text-black ring-2 ring-accent"
                    : "bg-white border border-gray-200 text-[#111111] hover:border-accent/50"
                }`}
              >
                <div className="flex items-center justify-center gap-1 mb-2">
                  <Calendar className="w-4 h-4" />
                  <span className="text-sm font-medium">Daily</span>
                </div>
                <span className="text-2xl font-bold">${equipment.dailyRate}</span>
                <span className="text-sm opacity-70">/day</span>
              </button>
              <button
                onClick={() => setRentalPeriod("weekly")}
                className={`p-4 rounded-xl text-center transition-all ${
                  rentalPeriod === "weekly"
                    ? "bg-accent text-black ring-2 ring-accent"
                    : "bg-white border border-gray-200 text-[#111111] hover:border-accent/50"
                }`}
              >
                <div className="flex items-center justify-center gap-1 mb-2">
                  <Calendar className="w-4 h-4" />
                  <span className="text-sm font-medium">Weekly</span>
                </div>
                <span className="text-2xl font-bold">${equipment.weeklyRate}</span>
                <span className="text-sm opacity-70">/wk</span>
              </button>
            </div>

            {/* Calculator */}
            <div className="bg-white border border-gray-200 rounded-2xl p-6 mb-8">
              <h3 className="text-[#111111] font-bold text-lg mb-4 flex items-center gap-2">
                <Calculator className="w-5 h-5 text-accent" />
                Rental Calculator
              </h3>
              <div className="grid sm:grid-cols-2 gap-4 mb-4">
                <div>
                  <label className="block text-[#666666] text-sm mb-2">Quantity</label>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => setQuantity(Math.max(1, quantity - 1))}
                      className="w-10 h-10 bg-[#F2F2F2] border border-gray-200 rounded-lg text-[#111111] hover:border-accent transition-colors"
                    >
                      -
                    </button>
                    <input
                      type="number"
                      value={quantity}
                      onChange={(e) =>
                        setQuantity(Math.max(1, parseInt(e.target.value) || 1))
                      }
                      className="w-full bg-white border border-gray-200 rounded-lg px-4 py-2 text-[#111111] text-center focus:outline-none focus:border-accent"
                    />
                    <button
                      onClick={() => setQuantity(quantity + 1)}
                      className="w-10 h-10 bg-[#F2F2F2] border border-gray-200 rounded-lg text-[#111111] hover:border-accent transition-colors"
                    >
                      +
                    </button>
                  </div>
                </div>
                <div>
                  <label className="block text-[#666666] text-sm mb-2">
                    Duration ({rentalPeriod === "hourly" ? "hours" : rentalPeriod === "daily" ? "days" : "weeks"})
                  </label>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => setDuration(Math.max(1, duration - 1))}
                      className="w-10 h-10 bg-[#F2F2F2] border border-gray-200 rounded-lg text-[#111111] hover:border-accent transition-colors"
                    >
                      -
                    </button>
                    <input
                      type="number"
                      value={duration}
                      onChange={(e) =>
                        setDuration(Math.max(1, parseInt(e.target.value) || 1))
                      }
                      className="w-full bg-white border border-gray-200 rounded-lg px-4 py-2 text-[#111111] text-center focus:outline-none focus:border-accent"
                    />
                    <button
                      onClick={() => setDuration(duration + 1)}
                      className="w-10 h-10 bg-[#F2F2F2] border border-gray-200 rounded-lg text-[#111111] hover:border-accent transition-colors"
                    >
                      +
                    </button>
                  </div>
                </div>
              </div>
              <div className="flex items-center justify-between pt-4 border-t border-gray-200">
                <span className="text-[#666666]">Estimated Total:</span>
                <span className="text-3xl font-bold text-accent">
                  ${calculateTotal()}
                </span>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-4 mb-8">
              <button
                disabled={!equipment.available}
                className={`flex-1 py-4 rounded-xl font-semibold text-lg transition-colors ${
                  equipment.available
                    ? "bg-accent hover:bg-[#C97F00] text-black"
                    : "bg-gray-200 text-[#999] cursor-not-allowed"
                }`}
              >
                {equipment.available ? "Book Now" : "Not Available"}
              </button>
              <button className="px-6 py-4 bg-white border border-gray-200 rounded-xl text-[#111111] hover:border-accent transition-colors">
                <Phone className="w-5 h-5" />
              </button>
            </div>

            {/* Features */}
            <div className="grid sm:grid-cols-3 gap-4">
              <div className="flex items-center gap-3 bg-white border border-gray-200 rounded-xl p-4">
                <Truck className="w-6 h-6 text-accent" />
                <div>
                  <p className="text-[#111111] font-medium">Free Delivery</p>
                  <p className="text-[#666666] text-sm">Orders over $200</p>
                </div>
              </div>
              <div className="flex items-center gap-3 bg-white border border-gray-200 rounded-xl p-4">
                <Shield className="w-6 h-6 text-accent" />
                <div>
                  <p className="text-[#111111] font-medium">Fully Insured</p>
                  <p className="text-[#666666] text-sm">Equipment coverage</p>
                </div>
              </div>
              <div className="flex items-center gap-3 bg-white border border-gray-200 rounded-xl p-4">
                <Clock className="w-6 h-6 text-accent" />
                <div>
                  <p className="text-[#111111] font-medium">24/7 Support</p>
                  <p className="text-[#666666] text-sm">Always available</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Specifications */}
        <div className="mt-16">
          <h2 className="text-2xl font-bold text-[#111111] mb-6">Specifications</h2>
          <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden">
            <div className="grid md:grid-cols-2">
              {Object.entries(equipment.specifications).map(([key, value], index) => (
                <div
                  key={key}
                  className={`flex justify-between p-4 ${
                    index % 2 === 0 ? "bg-white" : "bg-[#F2F2F2]"
                  } border-b border-gray-200 last:border-b-0`}
                >
                  <span className="text-[#666666]">{key}</span>
                  <span className="text-[#111111] font-medium">{value}</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Features */}
        <div className="mt-12">
          <h2 className="text-2xl font-bold text-[#111111] mb-6">Features</h2>
          <div className="grid sm:grid-cols-2 gap-4">
            {equipment.features.map((feature) => (
              <div
                key={feature}
                className="flex items-center gap-3 bg-white border border-gray-200 rounded-xl p-4"
              >
                <div className="w-8 h-8 bg-accent/20 rounded-full flex items-center justify-center">
                  <Check className="w-4 h-4 text-accent" />
                </div>
                <span className="text-[#111111]">{feature}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Related Equipment */}
        {relatedEquipment.length > 0 && (
          <div className="mt-16">
            <h2 className="text-2xl font-bold text-[#111111] mb-6">
              Related Equipment
            </h2>
            <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
              {relatedEquipment.map((item) => (
                <Link
                  key={item.id}
                  href={`/equipment/${item.id}`}
                  className="group bg-white border border-gray-200 rounded-2xl overflow-hidden hover:border-accent/50 hover:shadow-lg transition-all duration-300"
                >
                  <div className="relative h-40 overflow-hidden">
                    <img
                      src={item.image}
                      alt={item.name}
                      className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                    />
                  </div>
                  <div className="p-4">
                    <h3 className="text-[#111111] font-bold mb-2 line-clamp-1 group-hover:text-accent transition-colors">
                      {item.name}
                    </h3>
                    <p className="text-accent font-bold">
                      ${item.dailyRate}/day
                    </p>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
