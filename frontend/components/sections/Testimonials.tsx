"use client";

import { Star, Quote } from "lucide-react";

interface Testimonial {
  id: string;
  name: string;
  role: string;
  company: string;
  image: string;
  rating: number;
  text: string;
  category: string;
}

const TESTIMONIALS: Testimonial[] = [
  {
    id: "1",
    name: "John Mitchell",
    role: "Project Manager",
    company: "Mitchell Construction",
    image: "https://api.dicebear.com/7.x/avataaars/svg?seed=john",
    rating: 5,
    text: "Shelton Tool-Hire has been our go-to equipment supplier for over 3 years. Their construction equipment is always well-maintained, and the customer service is exceptional. The online calculator saved us hours of planning time.",
    category: "Equipment Performance",
  },
  {
    id: "2",
    name: "Sarah Thompson",
    role: "Owner",
    company: "Green Thumb Landscaping",
    image: "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
    rating: 5,
    text: "The landscaping tools I rent are always in perfect condition. Their support team really knows their stuff - they helped me pick the right equipment for a complex garden renovation project. Highly recommended!",
    category: "Support Services",
  },
  {
    id: "3",
    name: "Mike Rodriguez",
    role: "Lead Plumber",
    company: "Rodriguez Plumbing Co.",
    image: "https://api.dicebear.com/7.x/avataaars/svg?seed=mike",
    rating: 5,
    text: "Professional service from start to finish. The booking system is easy to use, prices are competitive, and when I had an issue with equipment on a Sunday, their after-hours support had it sorted within an hour.",
    category: "After Sales Support",
  },
  {
    id: "4",
    name: "Emma Wilson",
    role: "Interior Designer",
    company: "Wilson Interiors",
    image: "https://api.dicebear.com/7.x/avataaars/svg?seed=emma",
    rating: 5,
    text: "I rent decorating equipment regularly for client projects. The quality is consistent, and I love that I can calculate exact costs before booking. The delivery service is always on time too!",
    category: "Customer Service",
  },
];

export function Testimonials() {
  return (
    <section className="py-24 bg-white">
      <div className="max-w-7xl mx-auto px-4">
        {/* Section Header */}
        <div className="text-center mb-16">
          <h2 className="text-4xl md:text-5xl font-extrabold text-[#111111] mb-6">
            Trusted by Thousands
          </h2>
          <p className="text-[#666666] text-xl max-w-3xl mx-auto leading-relaxed">
            Don&apos;t just take our word for it — hear from professionals who trust
            us for their equipment needs.
          </p>
        </div>

        {/* Testimonials Grid */}
        <div className="grid md:grid-cols-2 gap-8 mb-16">
          {TESTIMONIALS.map((testimonial) => (
            <div
              key={testimonial.id}
              className="bg-gradient-to-br from-white to-[#FAFAFA] border-2 border-gray-200 rounded-3xl p-8 hover:border-accent/30 hover:shadow-xl transition-all duration-300"
            >
              {/* Header */}
              <div className="flex items-start justify-between mb-6">
                <div className="flex items-center gap-4">
                  <img
                    src={testimonial.image}
                    alt={testimonial.name}
                    className="w-16 h-16 rounded-full border-2 border-accent/20"
                  />
                  <div>
                    <h4 className="text-[#111111] font-bold text-lg">{testimonial.name}</h4>
                    <p className="text-[#666666] text-sm">
                      {testimonial.role} at {testimonial.company}
                    </p>
                  </div>
                </div>
                <Quote className="w-12 h-12 text-accent/20" />
              </div>

              {/* Rating & Category */}
              <div className="flex items-center gap-4 mb-4">
                <div className="flex items-center gap-1">
                  {[...Array(5)].map((_, i) => (
                    <Star
                      key={i}
                      className={`w-4 h-4 ${
                        i < testimonial.rating
                          ? "text-accent fill-accent"
                          : "text-gray"
                      }`}
                    />
                  ))}
                </div>
                <span className="text-xs text-accent bg-accent/10 px-3 py-1 rounded-full">
                  {testimonial.category}
                </span>
              </div>

              {/* Text */}
              <p className="text-[#666666] leading-relaxed text-base">{testimonial.text}</p>
            </div>
          ))}
        </div>

        {/* Stats Bar */}
        <div className="bg-gradient-to-br from-accent/5 to-accent/10 border-2 border-accent/20 rounded-3xl p-10">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="text-5xl font-extrabold text-accent mb-2">4.9</div>
              <div className="flex items-center justify-center gap-1 mb-2">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className="w-4 h-4 text-accent fill-accent"
                  />
                ))}
              </div>
              <p className="text-gray text-sm">Average Rating</p>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-[#111111] mb-2">15K+</div>
              <p className="text-[#111111] font-semibold">Total Reviews</p>
              <p className="text-[#666666] text-sm">From verified customers</p>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-[#111111] mb-2">98%</div>
              <p className="text-[#111111] font-semibold">Would Recommend</p>
              <p className="text-[#666666] text-sm">Customer satisfaction</p>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-[#111111] mb-2">5 min</div>
              <p className="text-[#111111] font-semibold">Avg Response Time</p>
              <p className="text-[#666666] text-sm">Support team</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
