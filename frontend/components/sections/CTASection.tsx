"use client";

import Link from "next/link";
import { Calculator, Calendar, Truck, ArrowRight } from "lucide-react";

export function CTASection() {
  return (
    <section className="py-20 bg-[#111111]">
      <div className="max-w-7xl mx-auto px-4">
        <div className="relative bg-[#1A1A1A] border border-white/10 rounded-3xl overflow-hidden">
          {/* Background Pattern */}
          <div className="absolute inset-0 opacity-10">
            <div
              className="absolute inset-0"
              style={{
                backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23000000' fill-opacity='1'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
              }}
            />
          </div>

          <div className="relative grid lg:grid-cols-2 gap-8 p-8 md:p-12 lg:p-16 items-center">
            {/* Content */}
            <div>
              <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
                Ready to Start Your Project?
              </h2>
              <p className="text-white/70 text-lg mb-8 max-w-lg">
                Use our rental calculator to estimate costs, choose your
                equipment, and book online in minutes. Delivery available!
              </p>

              <div className="flex flex-wrap gap-4">
                <Link
                  href="/calculator"
                  className="inline-flex items-center gap-2 bg-accent hover:bg-[#C97F00] text-black font-semibold px-8 py-4 rounded-lg transition-colors duration-200"
                >
                  <Calculator className="w-5 h-5" />
                  Calculate Rental Cost
                </Link>
                <Link
                  href="/contact"
                  className="inline-flex items-center gap-2 bg-transparent border-2 border-white/30 hover:border-white text-white font-semibold px-8 py-4 rounded-lg transition-colors duration-200"
                >
                  Contact Us
                  <ArrowRight className="w-5 h-5" />
                </Link>
              </div>
            </div>

            {/* Features */}
            <div className="grid sm:grid-cols-2 gap-4">
              <div className="bg-white/5 border border-white/10 rounded-xl p-5">
                <Calculator className="w-10 h-10 text-accent mb-3" />
                <h3 className="text-white font-bold text-lg mb-2">
                  Instant Pricing
                </h3>
                <p className="text-white/60 text-sm">
                  Calculate rental costs for hourly, daily, or weekly periods
                </p>
              </div>
              <div className="bg-white/5 border border-white/10 rounded-xl p-5">
                <Calendar className="w-10 h-10 text-accent mb-3" />
                <h3 className="text-white font-bold text-lg mb-2">
                  Easy Booking
                </h3>
                <p className="text-white/60 text-sm">
                  Book online 24/7 and manage your rentals anytime
                </p>
              </div>
              <div className="bg-white/5 border border-white/10 rounded-xl p-5">
                <Truck className="w-10 h-10 text-accent mb-3" />
                <h3 className="text-white font-bold text-lg mb-2">
                  Free Delivery
                </h3>
                <p className="text-white/60 text-sm">
                  Free delivery on orders over $200 within 20 miles
                </p>
              </div>
              <div className="bg-white/5 border border-white/10 rounded-xl p-5 flex flex-col justify-center items-center text-center">
                <div className="text-4xl font-bold text-accent mb-1">20%</div>
                <p className="text-white/60 text-sm">
                  <br />
                  with code FIRST20
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
