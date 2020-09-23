#pragma once
#include "QtreeNode.h"
#include "../Spatial.h"
#include <vector>

namespace spatial
{
    /**
     * A fast QuadTree implementation
     *  -) Linear SLAB Allocators for cheap dynamic growth
     *  -) Bulk collision reaction function
     *  -) Fast search via findNearby
     */
    class SPATIAL_API Qtree final : public Spatial
    {
        int Levels;
        int FullSize;
        int WorldSize;
        int SmallestCell;

        // Since we're not able to modify the tree while it's being built
        // Defer the split threshold setting to `rebuild` method
        int PendingSplitThreshold = QuadDefaultLeafSplitThreshold; // pending until next `rebuild()`
        int CurrentSplitThreshold = QuadDefaultLeafSplitThreshold; // actual value used
        QtreeNode* Root = nullptr;

        // NOTE: Cannot use std::unique_ptr here due to dll-interface
        SlabAllocator* FrontAlloc = new SlabAllocator{AllocatorSlabSize};
        SlabAllocator* BackAlloc  = new SlabAllocator{AllocatorSlabSize};

        std::vector<SpatialObject> Objects;
        std::vector<SpatialObject> Pending;

    public:

        /**
         * @param worldSize The Width and Height of the simulation world
         * @param smallestCell The smallest allowed Qtree node size to prevent qtree getting too deep
         */
        explicit Qtree(int worldSize, int smallestCell);
        ~Qtree();
        
        SpatialType type() const override { return SpatialType::QuadTree; }
        const char* name() const override { return "Qtree"; }
        uint32_t totalMemory() const override;
        int fullSize() const override { return FullSize; }
        int worldSize() const override { return WorldSize; }
        int count() const override { return (int)Objects.size(); }
        const SpatialObject& get(int objectId) const override { return Objects[objectId]; }
        
        int nodeCapacity() const override { return PendingSplitThreshold; }
        void nodeCapacity(int capacity) override { PendingSplitThreshold = capacity; }
        int smallestCellSize() const override { return SmallestCell; }
        void smallestCellSize(int cellSize) override;

        void clear() override;
        void rebuild() override;
        int insert(const SpatialObject& o) override;
        void update(int objectId, int x, int y) override;
        void remove(int objectId) override;
        using Spatial::collideAll;
        void collideAll(float timeStep, void* user, CollisionFunc onCollide) override;
        int findNearby(int* outResults, const SearchOptions& opt) const override;
        void debugVisualize(const VisualizerOptions& opt, Visualizer& visualizer) const override;

    private:

        QtreeNode* createRoot() const;
        void insertAt(int level, QtreeNode& root, SpatialObject* o);
        void insertAtLeaf(int level, QtreeNode& leaf, SpatialObject* o);
        void removeAt(QtreeNode* root, int objectId);
        void markForRemoval(int objectId, SpatialObject& o);
    };
}
