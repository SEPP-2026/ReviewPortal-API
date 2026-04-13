"use client";

import { useState } from "react";
import { Star, ThumbsUp, MessageCircle, Filter } from "lucide-react";

interface Review {
  id: string;
  author: string;
  avatar: string;
  role: string;
  date: string;
  rating: number;
  category: string;
  equipment?: string;
  title: string;
  text: string;
  helpful: number;
  replies: number;
}

const REVIEWS: Review[] = [
  {
    id: "1",
    author: "John Mitchell",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=john",
    role: "Construction Manager",
    date: "2 days ago",
    rating: 5,
    category: "Equipment Performance",
    equipment: "Concrete Mixer 120L",
    title: "Excellent equipment, reliable performance",
    text: "I've rented the concrete mixer multiple times for various projects. It's always well-maintained and performs flawlessly. The mixing capacity is perfect for medium-sized jobs, and the electric motor is powerful enough for consistent results.",
    helpful: 24,
    replies: 3,
  },
  {
    id: "2",
    author: "Sarah Thompson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
    role: "Landscape Designer",
    date: "1 week ago",
    rating: 5,
    category: "Customer Service",
    equipment: "Honda Ride-On Mower",
    title: "Outstanding customer service experience",
    text: "The team at Shelton Tool-Hire went above and beyond. When I needed to extend my rental, they accommodated me immediately. The ride-on mower was delivered on time and picked up exactly when promised. Will definitely use again!",
    helpful: 18,
    replies: 2,
  },
  {
    id: "3",
    author: "Mike Rodriguez",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=mike",
    role: "Plumbing Contractor",
    date: "2 weeks ago",
    rating: 5,
    category: "Support Services",
    equipment: "Electric Drain Cleaner",
    title: "Technical support saved my job",
    text: "Had an issue on a Sunday evening with the drain cleaner. The 24/7 support line answered immediately and walked me through the troubleshooting. Problem solved in 10 minutes. This level of support is invaluable for professionals like me.",
    helpful: 31,
    replies: 5,
  },
  {
    id: "4",
    author: "Emma Wilson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=emma",
    role: "Interior Designer",
    date: "3 weeks ago",
    rating: 4,
    category: "Equipment Performance",
    equipment: "Wagner Paint Sprayer Pro",
    title: "Great results, slight learning curve",
    text: "The paint sprayer delivered professional-quality results. It took me about 30 minutes to get the hang of it, but once I did, the finish was amazing. Would recommend watching their tutorial video first.",
    helpful: 12,
    replies: 1,
  },
  {
    id: "5",
    author: "David Chen",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=david",
    role: "Property Manager",
    date: "1 month ago",
    rating: 5,
    category: "After Sales Support",
    title: "Reliable partner for property maintenance",
    text: "We manage 50+ properties and rely on Shelton Tool-Hire for all our equipment needs. Their corporate account pricing is excellent, and the equipment is always ready when we need it. The after-sales support has been fantastic too.",
    helpful: 45,
    replies: 8,
  },
  {
    id: "6",
    author: "Lisa Anderson",
    avatar: "https://api.dicebear.com/7.x/avataaars/svg?seed=lisa",
    role: "DIY Enthusiast",
    date: "1 month ago",
    rating: 5,
    category: "Customer Service",
    equipment: "Kärcher Pressure Washer Pro",
    title: "First rental experience was perfect",
    text: "As a first-time renter, I was nervous about the process. The staff were incredibly helpful explaining how to use the pressure washer safely. The online booking was easy, and the pricing was very transparent. Fantastic experience!",
    helpful: 22,
    replies: 4,
  },
];

const CATEGORIES = [
  "All Reviews",
  "Equipment Performance",
  "Customer Service",
  "Support Services",
  "After Sales Support",
];

export default function ReviewsPage() {
  const [selectedCategory, setSelectedCategory] = useState("All Reviews");
  const [sortBy, setSortBy] = useState<"recent" | "helpful">("recent");

  const filteredReviews = REVIEWS.filter((review) =>
    selectedCategory === "All Reviews"
      ? true
      : review.category === selectedCategory
  ).sort((a, b) => {
    if (sortBy === "helpful") return b.helpful - a.helpful;
    return 0; // Keep original order for recent
  });

  const averageRating =
    REVIEWS.reduce((acc, r) => acc + r.rating, 0) / REVIEWS.length;

  return (
    <div className="min-h-screen bg-[#F2F2F2] pt-40 pb-20">
      <div className="max-w-7xl mx-auto px-4">
        {/* Header */}
        <div className="text-center mb-12">
          <span className="inline-block text-accent font-semibold text-sm uppercase tracking-wider mb-4">
            Customer Feedback
          </span>
          <h1 className="text-4xl md:text-5xl font-bold text-[#111111] mb-4">
            Reviews & Ratings
          </h1>
          <p className="text-[#666666] text-lg max-w-2xl mx-auto">
            See what our customers have to say about their rental experience
            with Shelton Tool-Hire.
          </p>
        </div>

        {/* Stats */}
        <div className="bg-white border border-gray-200 rounded-2xl p-8 mb-12">
          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="text-5xl font-bold text-accent mb-2">
                {averageRating.toFixed(1)}
              </div>
              <div className="flex justify-center gap-1 mb-2">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className={`w-5 h-5 ${
                      i < Math.floor(averageRating)
                        ? "text-accent fill-accent"
                        : "text-gray"
                    }`}
                  />
                ))}
              </div>
              <p className="text-gray">Average Rating</p>
            </div>
            <div className="text-center">
              <div className="text-5xl font-bold text-[#111111] mb-2">
                {REVIEWS.length}K+
              </div>
              <p className="text-[#111111] font-semibold">Total Reviews</p>
              <p className="text-[#666666] text-sm">From verified customers</p>
            </div>
            <div className="text-center">
              <div className="text-5xl font-bold text-[#111111] mb-2">98%</div>
              <p className="text-[#111111] font-semibold">Recommend Us</p>
              <p className="text-[#666666] text-sm">Customer satisfaction</p>
            </div>
            <div className="text-center">
              <div className="text-5xl font-bold text-[#111111] mb-2">24hr</div>
              <p className="text-[#111111] font-semibold">Response Time</p>
              <p className="text-[#666666] text-sm">For all reviews</p>
            </div>
          </div>
        </div>

        {/* Filters */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
          <div className="flex flex-wrap gap-2">
            {CATEGORIES.map((category) => (
              <button
                key={category}
                onClick={() => setSelectedCategory(category)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                  selectedCategory === category
                    ? "bg-accent text-black"
                    : "bg-white text-[#666666] hover:text-[#111111] border border-gray-200"
                }`}
              >
                {category}
              </button>
            ))}
          </div>
          <div className="flex items-center gap-2">
            <Filter className="w-5 h-5 text-[#666666]" />
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as "recent" | "helpful")}
              className="bg-white border border-gray-200 rounded-lg px-4 py-2 text-[#111111] text-sm focus:outline-none focus:border-accent"
            >
              <option value="recent">Most Recent</option>
              <option value="helpful">Most Helpful</option>
            </select>
          </div>
        </div>

        {/* Reviews List */}
        <div className="space-y-6">
          {filteredReviews.map((review) => (
            <div
              key={review.id}
              className="bg-white border border-gray-200 rounded-2xl p-6 hover:border-accent/30 hover:shadow-lg transition-all"
            >
              {/* Header */}
              <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4 mb-4">
                <div className="flex items-center gap-4">
                  <img
                    src={review.avatar}
                    alt={review.author}
                    className="w-14 h-14 rounded-full bg-gray/20"
                  />
                  <div>
                    <h4 className="text-[#111111] font-bold">{review.author}</h4>
                    <p className="text-[#666666] text-sm">{review.role}</p>
                    <p className="text-[#666666] text-xs">{review.date}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <div className="flex items-center gap-1">
                    {[...Array(5)].map((_, i) => (
                      <Star
                        key={i}
                        className={`w-4 h-4 ${
                          i < review.rating
                            ? "text-accent fill-accent"
                            : "text-gray"
                        }`}
                      />
                    ))}
                  </div>
                  <span className="text-xs text-accent bg-accent/10 px-3 py-1 rounded-full">
                    {review.category}
                  </span>
                </div>
              </div>

              {/* Equipment Tag */}
              {review.equipment && (
                <div className="mb-3">
                    <span className="text-xs text-[#666666] bg-[#F2F2F2] px-3 py-1 rounded-lg">
                    Equipment: {review.equipment}
                  </span>
                </div>
              )}

              {/* Content */}
              <h3 className="text-lg font-bold text-[#111111] mb-2">
                {review.title}
              </h3>
              <p className="text-[#666666] leading-relaxed mb-4">{review.text}</p>

              {/* Actions */}
              <div className="flex items-center gap-6 pt-4 border-t border-gray-200">
                <button className="flex items-center gap-2 text-[#666666] hover:text-accent transition-colors">
                  <ThumbsUp className="w-4 h-4" />
                  <span className="text-sm">Helpful ({review.helpful})</span>
                </button>
                <button className="flex items-center gap-2 text-[#666666] hover:text-accent transition-colors">
                  <MessageCircle className="w-4 h-4" />
                  <span className="text-sm">Replies ({review.replies})</span>
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Load More */}
        <div className="text-center mt-12">
          <button className="bg-accent/10 hover:bg-accent text-accent hover:text-black font-semibold px-8 py-4 rounded-lg transition-colors">
            Load More Reviews
          </button>
        </div>
      </div>
    </div>
  );
}
