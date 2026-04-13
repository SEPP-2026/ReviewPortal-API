"use client";

import { useState, useMemo } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import {
  Search,
  Filter,
  Star,
  Clock,
  ChevronDown,
  Grid3X3,
  List,
} from "lucide-react";
import { EQUIPMENT_DATA, CATEGORIES } from "@/lib/equipment-data";

type SortOption = "featured" | "price-low" | "price-high" | "rating" | "name";
type ViewMode = "grid" | "list";

export function EquipmentCatalogue() {
  const searchParams = useSearchParams();
  const initialCategory = searchParams.get("category") || "all";

  const [searchQuery, setSearchQuery] = useState("");
  const [selectedCategory, setSelectedCategory] = useState(initialCategory);
  const [sortBy, setSortBy] = useState<SortOption>("featured");
  const [viewMode, setViewMode] = useState<ViewMode>("grid");
  const [showFilters, setShowFilters] = useState(false);
  const [availableOnly, setAvailableOnly] = useState(false);

  const filteredEquipment = useMemo(() => {
    let result = EQUIPMENT_DATA;

    // Filter by category
    if (selectedCategory !== "all") {
      result = result.filter(
        (item) => item.categorySlug === selectedCategory
      );
    }

    // Filter by search query
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      result = result.filter(
        (item) =>
          item.name.toLowerCase().includes(query) ||
          item.description.toLowerCase().includes(query) ||
          item.category.toLowerCase().includes(query)
      );
    }

    // Filter by availability
    if (availableOnly) {
      result = result.filter((item) => item.available);
    }

    // Sort
    switch (sortBy) {
      case "price-low":
        result = [...result].sort((a, b) => a.dailyRate - b.dailyRate);
        break;
      case "price-high":
        result = [...result].sort((a, b) => b.dailyRate - a.dailyRate);
        break;
      case "rating":
        result = [...result].sort((a, b) => b.rating - a.rating);
        break;
      case "name":
        result = [...result].sort((a, b) => a.name.localeCompare(b.name));
        break;
      default:
        // featured - keep original order
        break;
    }

    return result;
  }, [searchQuery, selectedCategory, sortBy, availableOnly]);

  return (
    <section className="min-h-screen bg-[#F2F2F2] pt-40 pb-20">
      <div className="max-w-7xl mx-auto px-4">
        {/* Header */}
        <div className="mb-10">
          <h1 className="text-3xl md:text-4xl font-bold text-[#111111] mb-4">
            Equipment Catalogue
          </h1>
          <p className="text-[#666666] text-lg">
            Browse our selection of professional tools and equipment for rent.
          </p>
        </div>

        {/* Search and Filters Bar */}
        <div className="bg-white border border-gray-200 rounded-2xl p-4 mb-8">
          <div className="flex flex-col lg:flex-row gap-4">
            {/* Search */}
            <div className="flex-1 relative">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-[#999]" />
              <input
                type="text"
                placeholder="Search equipment..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full bg-white border border-gray-200 rounded-xl pl-12 pr-4 py-3 text-[#111111] placeholder:text-[#999] focus:outline-none focus:border-accent transition-colors"
              />
            </div>

            {/* Desktop Category Pills */}
            <div className="hidden lg:flex items-center gap-2 overflow-x-auto">
              {CATEGORIES.map((category) => (
                <button
                  key={category.slug}
                  onClick={() => setSelectedCategory(category.slug)}
                  className={`px-4 py-2 rounded-lg font-medium whitespace-nowrap transition-colors ${
                    selectedCategory === category.slug
                      ? "bg-accent text-black"
                      : "bg-[#F2F2F2] text-[#666666] hover:text-[#111111] border border-gray-200"
                  }`}
                >
                  {category.name}
                </button>
              ))}
            </div>

            {/* Mobile Filter Button */}
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="lg:hidden flex items-center justify-center gap-2 bg-white border border-gray-200 rounded-xl px-4 py-3 text-[#111111]"
            >
              <Filter className="w-5 h-5" />
              Filters
              <ChevronDown
                className={`w-4 h-4 transition-transform ${
                  showFilters ? "rotate-180" : ""
                }`}
              />
            </button>
          </div>

          {/* Mobile Filters Panel */}
          {showFilters && (
            <div className="lg:hidden mt-4 pt-4 border-t border-gray-200 space-y-4">
              {/* Category Select */}
              <div>
                <label className="block text-sm text-[#666666] mb-2">Category</label>
                <div className="flex flex-wrap gap-2">
                  {CATEGORIES.map((category) => (
                    <button
                      key={category.slug}
                      onClick={() => setSelectedCategory(category.slug)}
                      className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                        selectedCategory === category.slug
                          ? "bg-accent text-black"
                          : "bg-[#F2F2F2] text-[#666666] border border-gray-200"
                      }`}
                    >
                      {category.name}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Sort and View Controls */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <p className="text-[#666666]">
            Showing{" "}
            <span className="text-[#111111] font-semibold">
              {filteredEquipment.length}
            </span>{" "}
            results
          </p>

          <div className="flex items-center gap-4">
            {/* Available Only Toggle */}
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={availableOnly}
                onChange={(e) => setAvailableOnly(e.target.checked)}
                className="w-4 h-4 rounded border-gray-300 bg-white text-accent focus:ring-accent"
              />
              <span className="text-[#666666] text-sm">Available only</span>
            </label>

            {/* Sort Dropdown */}
            <div className="relative">
              <select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value as SortOption)}
                className="appearance-none bg-white border border-gray-200 rounded-lg pl-4 pr-10 py-2 text-[#111111] text-sm focus:outline-none focus:border-accent cursor-pointer"
              >
                <option value="featured">Featured</option>
                <option value="price-low">Price: Low to High</option>
                <option value="price-high">Price: High to Low</option>
                <option value="rating">Top Rated</option>
                <option value="name">Name A-Z</option>
              </select>
              <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[#999] pointer-events-none" />
            </div>

            {/* View Mode Toggle */}
            <div className="hidden sm:flex items-center bg-white border border-gray-200 rounded-lg p-1">
              <button
                onClick={() => setViewMode("grid")}
                className={`p-2 rounded-md transition-colors ${
                  viewMode === "grid"
                    ? "bg-accent text-black"
                    : "text-[#666666] hover:text-[#111111]"
                }`}
              >
                <Grid3X3 className="w-4 h-4" />
              </button>
              <button
                onClick={() => setViewMode("list")}
                className={`p-2 rounded-md transition-colors ${
                  viewMode === "list"
                    ? "bg-accent text-black"
                    : "text-[#666666] hover:text-[#111111]"
                }`}
              >
                <List className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        {/* Equipment Grid */}
        {filteredEquipment.length > 0 ? (
          <div
            className={
              viewMode === "grid"
                ? "grid sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6"
                : "flex flex-col gap-4"
            }
          >
            {filteredEquipment.map((item) => (
              <Link
                key={item.id}
                href={`/equipment/${item.id}`}
                className={`group bg-white border border-gray-200 rounded-2xl overflow-hidden hover:border-accent/50 hover:shadow-lg transition-all duration-300 hover:translate-y-[-4px] ${
                  viewMode === "list" ? "flex" : ""
                }`}
              >
                {/* Image */}
                <div
                  className={`relative overflow-hidden ${
                    viewMode === "list" ? "w-48 shrink-0" : "h-48"
                  }`}
                >
                  <img
                    src={item.image}
                    alt={item.name}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                  />
                  <div className="absolute top-3 left-3">
                    <span className="bg-accent text-black text-xs font-semibold px-2 py-1 rounded-full">
                      {item.category}
                    </span>
                  </div>
                  <div className="absolute top-3 right-3">
                    <span
                      className={`text-xs font-semibold px-2 py-1 rounded-full ${
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
                <div className={`p-4 ${viewMode === "list" ? "flex-1" : ""}`}>
                  <div className="flex items-center gap-2 mb-2">
                    <Star className="w-4 h-4 text-accent fill-accent" />
                    <span className="text-[#111111] font-semibold text-sm">
                      {item.rating}
                    </span>
                    <span className="text-[#666666] text-sm">
                      ({item.reviewCount})
                    </span>
                  </div>

                  <h3 className="text-lg font-bold text-[#111111] mb-2 group-hover:text-accent transition-colors duration-300 line-clamp-1">
                    {item.name}
                  </h3>

                  {viewMode === "list" && (
                    <p className="text-[#666666] text-sm mb-3 line-clamp-2">
                      {item.description}
                    </p>
                  )}

                  {/* Pricing */}
                  <div
                    className={`grid grid-cols-3 gap-2 ${
                      viewMode === "list" ? "max-w-xs" : "mb-4"
                    }`}
                  >
                    <div className="bg-[#F2F2F2] rounded-lg p-2 text-center">
                      <div className="flex items-center justify-center gap-1 text-[#666666] text-xs mb-1">
                        <Clock className="w-3 h-3" />
                        Hour
                      </div>
                      <span className="text-accent font-bold text-sm">
                        ${item.hourlyRate}
                      </span>
                    </div>
                    <div className="bg-[#F2F2F2] rounded-lg p-2 text-center">
                      <div className="text-[#666666] text-xs mb-1">Day</div>
                      <span className="text-accent font-bold text-sm">
                        ${item.dailyRate}
                      </span>
                    </div>
                    <div className="bg-[#F2F2F2] rounded-lg p-2 text-center">
                      <div className="text-[#666666] text-xs mb-1">Week</div>
                      <span className="text-accent font-bold text-sm">
                        ${item.weeklyRate}
                      </span>
                    </div>
                  </div>

                  {viewMode === "grid" && (
                    <button className="w-full bg-accent/10 hover:bg-accent text-accent hover:text-black font-semibold py-2.5 rounded-lg transition-colors duration-300 text-sm">
                      View Details
                    </button>
                  )}
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <div className="text-center py-20">
            <div className="text-gray text-6xl mb-4">🔍</div>
            <h3 className="text-xl font-bold text-[#111111] mb-2">
              No equipment found
            </h3>
            <p className="text-[#666666] mb-6">
              Try adjusting your search or filter criteria
            </p>
            <button
              onClick={() => {
                setSearchQuery("");
                setSelectedCategory("all");
                setAvailableOnly(false);
              }}
              className="bg-accent hover:bg-[#C97F00] text-black font-semibold px-6 py-3 rounded-lg transition-colors"
            >
              Clear Filters
            </button>
          </div>
        )}
      </div>
    </section>
  );
}
